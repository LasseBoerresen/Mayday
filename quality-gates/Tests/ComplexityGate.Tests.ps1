#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
# Tests for quality-gates/complexity-gate.ps1.
#
# Each test builds a scratch C# project with the real `dotnet build`, because the behavior under test is what the
# CA1502 analyzer reports for the files in quality-gates/install. Names follow the repo's Given/When/Then convention.

BeforeAll {
    $script:Root = Join-Path $PSScriptRoot '..' | Resolve-Path | Select-Object -ExpandProperty Path
    $script:Gate = Join-Path $script:Root 'complexity-gate.ps1'

    # A method with `$Ifs` independent branches has cyclomatic complexity $Ifs + 1.
    function New-FixtureProject([int] $Ifs) {
        $dir = Join-Path $TestDrive "proj-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Path $dir | Out-Null
        Copy-Item (Join-Path $script:Root 'install\*') -Destination $dir -Force -Include '*', '.editorconfig'
        Set-Content -Path (Join-Path $dir 'Fixture.csproj') -Value @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Library</OutputType></PropertyGroup>
</Project>
'@
        $branches = (1..$Ifs | ForEach-Object { "        if (x == $_) { y += $_; }" }) -join "`n"
        Set-Content -Path (Join-Path $dir 'Branchy.cs') -Value @"
public static class Branchy
{
    public static int Compute(int x)
    {
        var y = 0;
$branches
        return y;
    }
}
"@
        $dir
    }

    function Invoke-Gate([string] $ProjectRoot) {
        $ErrorActionPreference = 'Continue'
        $output = '' | & powershell -NoProfile -ExecutionPolicy Bypass -File $script:Gate -ProjectRoot $ProjectRoot 2>&1 | Out-String
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output }
    }
}

Describe 'complexity-gate.ps1' {
    It 'GivenMethodAboveThreshold_WhenRun_ThenExitsTwoNamingTheMethod' {
        # Given
        $project = New-FixtureProject -Ifs 12 # complexity 13

        # When
        $result = Invoke-Gate $project

        # Then
        $result.ExitCode | Should -Be 2
        $result.Output | Should -Match "Compute.*cyclomatic complexity of '13'"
    }

    It 'GivenMethodAtThreshold_WhenRun_ThenExitsZero' {
        # Given
        $project = New-FixtureProject -Ifs 11 # complexity 12

        # When
        $result = Invoke-Gate $project

        # Then
        $result.ExitCode | Should -Be 0
    }

    It 'GivenCompileError_WhenRun_ThenExitsTwoReportingTheBuildFailure' {
        # Given
        $project = New-FixtureProject -Ifs 1
        Add-Content -Path (Join-Path $project 'Branchy.cs') -Value 'class Broken { int M() { return "text"; } }'

        # When
        $result = Invoke-Gate $project

        # Then
        $result.ExitCode | Should -Be 2
        $result.Output | Should -Match 'Build failed'
        $result.Output | Should -Match 'CS0029'
    }
}
