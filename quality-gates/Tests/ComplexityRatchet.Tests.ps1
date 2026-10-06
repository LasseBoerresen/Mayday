#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
# Tests for the baseline ("ratchet") mode of quality-gates/complexity-gate.ps1.
#
# With complexity-baseline.json present the gate tolerates the recorded violations and fails only on new ones or on a
# recorded method that got more complex. Each test builds a scratch C# project with the real `dotnet build`, with
# CA1502 at severity warning as ratchet mode requires. Names follow the repo's Given/When/Then convention.

BeforeAll {
    $script:Root = Join-Path $PSScriptRoot '..' | Resolve-Path | Select-Object -ExpandProperty Path
    $script:Gate = Join-Path $script:Root 'complexity-gate.ps1'

    # $Methods: file name -> list of @{ Name; Ifs }. A method with Ifs branches has complexity Ifs + 1. Methods of the
    # same name in one file become overloads.
    function New-RatchetProject([hashtable] $Methods, [string] $Severity = 'warning', [switch] $TreatWarningsAsErrors) {
        $dir = Join-Path $TestDrive "proj-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Path $dir | Out-Null
        Copy-Item (Join-Path $script:Root 'install\*') -Destination $dir -Force -Include '*', '.editorconfig'
        Set-Content -Path (Join-Path $dir '.editorconfig') -Value "[*.cs]`ndotnet_diagnostic.CA1502.severity = $Severity"
        $twae = "$TreatWarningsAsErrors".ToLower()
        Set-Content -Path (Join-Path $dir 'Fixture.csproj') -Value @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Library</OutputType><TreatWarningsAsErrors>$twae</TreatWarningsAsErrors></PropertyGroup>
</Project>
"@
        foreach ($file in $Methods.Keys) {
            $seen = @{}
            $bodies = foreach ($m in $Methods[$file]) {
                $n = if ($seen.ContainsKey($m.Name)) { $seen[$m.Name] + 1 } else { 0 }
                $seen[$m.Name] = $n
                $extra = (1..$n | ForEach-Object { ", int p$_" }) -join ''
                $branches = (1..$m.Ifs | ForEach-Object { "        if (x == $_) { y += $_; }" }) -join "`n"
                "    public static int $($m.Name)(int x$extra)`n    {`n        var y = 0;`n$branches`n        return y;`n    }"
            }
            $class = [IO.Path]::GetFileNameWithoutExtension($file)
            Set-Content -Path (Join-Path $dir $file) -Value "public static class $class`n{`n$($bodies -join "`n")`n}"
        }
        $dir
    }

    function Set-Baseline([string] $Project, [hashtable] $Methods) {
        @{ methods = $Methods } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $Project 'complexity-baseline.json')
    }

    function Get-Baseline([string] $Project) {
        (Get-Content (Join-Path $Project 'complexity-baseline.json') -Raw | ConvertFrom-Json).methods
    }

    function Invoke-Gate([string] $ProjectRoot, [string[]] $Arguments = @()) {
        $ErrorActionPreference = 'Continue'
        $output = '' | & powershell -NoProfile -ExecutionPolicy Bypass -File $script:Gate -ProjectRoot $ProjectRoot @Arguments 2>&1 | Out-String
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output }
    }
}

Describe 'complexity-gate.ps1 with a baseline' {
    It 'GivenBaselinedViolationUnchanged_WhenRun_ThenExitsZero' {
        # Given
        $project = New-RatchetProject @{ 'Branchy.cs' = @(@{ Name = 'Compute'; Ifs = 13 }) } # complexity 14
        Set-Baseline $project @{ 'Branchy.cs::Compute' = @(14) }

        # When
        $result = Invoke-Gate $project

        # Then
        $result.ExitCode | Should -Be 0
    }

    It 'GivenBaselinedViolationThatGrew_WhenRun_ThenExitsTwoNamingMethodAndBothValues' {
        # Given
        $project = New-RatchetProject @{ 'Branchy.cs' = @(@{ Name = 'Compute'; Ifs = 14 }) } # complexity 15
        Set-Baseline $project @{ 'Branchy.cs::Compute' = @(14) }

        # When
        $result = Invoke-Gate $project

        # Then
        $result.ExitCode | Should -Be 2
        $result.Output | Should -Match 'Branchy\.cs::Compute'
        $result.Output | Should -Match '14'
        $result.Output | Should -Match '15'
    }

    It 'GivenNewViolationNotInBaseline_WhenRun_ThenExitsTwoNamingMethod' {
        # Given
        $project = New-RatchetProject @{ 'Branchy.cs' = @(@{ Name = 'Compute'; Ifs = 13 }); 'Other.cs' = @(@{ Name = 'Fresh'; Ifs = 12 }) }
        Set-Baseline $project @{ 'Branchy.cs::Compute' = @(14) }

        # When
        $result = Invoke-Gate $project

        # Then
        $result.ExitCode | Should -Be 2
        $result.Output | Should -Match 'Other\.cs::Fresh'
    }

    It 'GivenBaselinedViolationThatImproved_WhenRun_ThenExitsZeroSuggestingToLowerTheBaseline' {
        # Given
        $project = New-RatchetProject @{ 'Branchy.cs' = @(@{ Name = 'Compute'; Ifs = 5 }) } # complexity 6, under the limit
        Set-Baseline $project @{ 'Branchy.cs::Compute' = @(14) }

        # When
        $result = Invoke-Gate $project

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match '-UpdateBaseline'
    }

    It 'GivenOverloadsWhenOneGrew_WhenRun_ThenExitsTwo' {
        # Given: baseline records both overloads of Compute (14 and 13); one grows to 15
        $project = New-RatchetProject @{ 'Branchy.cs' = @(@{ Name = 'Compute'; Ifs = 14 }, @{ Name = 'Compute'; Ifs = 12 }) }
        Set-Baseline $project @{ 'Branchy.cs::Compute' = @(14, 13) }

        # When
        $result = Invoke-Gate $project

        # Then
        $result.ExitCode | Should -Be 2
    }

    It 'GivenProjectTreatingWarningsAsErrors_WhenRunWithBaselinedViolation_ThenExitsZero' {
        # Given
        $project = New-RatchetProject @{ 'Branchy.cs' = @(@{ Name = 'Compute'; Ifs = 13 }) } -TreatWarningsAsErrors
        Set-Baseline $project @{ 'Branchy.cs::Compute' = @(14) }

        # When
        $result = Invoke-Gate $project

        # Then
        $result.ExitCode | Should -Be 0
    }
}

Describe 'complexity-gate.ps1 baseline maintenance' {
    It 'GivenNoBaseline_WhenInitBaseline_ThenWritesCurrentViolationsAndGatePasses' {
        # Given
        $project = New-RatchetProject @{ 'Branchy.cs' = @(@{ Name = 'Compute'; Ifs = 13 }, @{ Name = 'Small'; Ifs = 2 }) }

        # When
        $init = Invoke-Gate $project @('-InitBaseline')
        $after = Invoke-Gate $project

        # Then
        $init.ExitCode | Should -Be 0
        @((Get-Baseline $project).PSObject.Properties.Name) | Should -Be @('Branchy.cs::Compute')
        @((Get-Baseline $project).'Branchy.cs::Compute') | Should -Be @(14)
        $after.ExitCode | Should -Be 0
    }

    It 'GivenExistingBaseline_WhenInitBaseline_ThenRefusesAndKeepsIt' {
        # Given
        $project = New-RatchetProject @{ 'Branchy.cs' = @(@{ Name = 'Compute'; Ifs = 13 }) }
        Set-Baseline $project @{ 'Branchy.cs::Compute' = @(20) }

        # When
        $result = Invoke-Gate $project @('-InitBaseline')

        # Then
        $result.ExitCode | Should -Be 2
        @((Get-Baseline $project).'Branchy.cs::Compute') | Should -Be @(20)
    }

    It 'GivenImprovedAndFixedMethods_WhenUpdateBaseline_ThenLowersOneAndDropsTheOther' {
        # Given
        $project = New-RatchetProject @{ 'Branchy.cs' = @(@{ Name = 'Compute'; Ifs = 13 }, @{ Name = 'Fixed'; Ifs = 3 }) }
        Set-Baseline $project @{ 'Branchy.cs::Compute' = @(20); 'Branchy.cs::Fixed' = @(15) }

        # When
        $result = Invoke-Gate $project @('-UpdateBaseline')

        # Then
        $result.ExitCode | Should -Be 0
        @((Get-Baseline $project).PSObject.Properties.Name) | Should -Be @('Branchy.cs::Compute')
        @((Get-Baseline $project).'Branchy.cs::Compute') | Should -Be @(14)
    }

    It 'GivenRegression_WhenUpdateBaseline_ThenExitsTwoAndLeavesBaselineUnchanged' {
        # Given
        $project = New-RatchetProject @{ 'Branchy.cs' = @(@{ Name = 'Compute'; Ifs = 14 }) } # complexity 15
        Set-Baseline $project @{ 'Branchy.cs::Compute' = @(14) }

        # When
        $result = Invoke-Gate $project @('-UpdateBaseline')

        # Then
        $result.ExitCode | Should -Be 2
        @((Get-Baseline $project).'Branchy.cs::Compute') | Should -Be @(14)
    }
}
