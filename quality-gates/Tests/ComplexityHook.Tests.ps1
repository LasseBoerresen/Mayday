#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
# Tests for quality-gates/complexity-hook.ps1.
#
# The gate is replaced by a script that logs its call and exits with a chosen code, so no project is built.
# Names follow the repo's Given/When/Then convention.

BeforeAll {
    $script:Hook = Join-Path $PSScriptRoot '..\complexity-hook.ps1' | Resolve-Path | Select-Object -ExpandProperty Path

    # Runs the hook in a child PowerShell with the given stdin; returns its exit code and its stderr as Claude Code
    # sees it (a pipeline with 2>&1 would decorate it with error records).
    function Invoke-HookRaw([string] $StdIn) {
        $psi = New-Object Diagnostics.ProcessStartInfo
        $psi.FileName = 'powershell'
        $psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$script:Hook`" -GateScript `"$script:FakeGate`" -ProjectRoot C:\repo"
        $psi.UseShellExecute = $false
        $psi.RedirectStandardInput = $true
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $hook = [Diagnostics.Process]::Start($psi)
        $hook.StandardInput.Write($StdIn)
        $hook.StandardInput.Close()
        $stdout = $hook.StandardOutput.ReadToEndAsync()
        $stderr = $hook.StandardError.ReadToEndAsync()
        $hook.WaitForExit()
        [pscustomobject]@{ ExitCode = $hook.ExitCode; Output = $stderr.Result + $stdout.Result }
    }

    function Invoke-Hook([string] $Tool, [string] $FilePath) {
        Invoke-HookRaw (@{ tool_name = $Tool; tool_input = @{ file_path = $FilePath } } | ConvertTo-Json -Compress)
    }
}

Describe 'complexity-hook.ps1' {
    BeforeEach {
        $script:Log = Join-Path $TestDrive "log-$([guid]::NewGuid().ToString('N')).txt"
        $env:FAKE_GATE_LOG = $script:Log
        $env:FAKE_GATE_EXIT = '0'
        $script:FakeGate = Join-Path $TestDrive "fake-gate-$([guid]::NewGuid().ToString('N')).ps1"
        Set-Content -Path $script:FakeGate -Value @'
param($ProjectRoot)
Add-Content -Path $env:FAKE_GATE_LOG -Value "gate $ProjectRoot"
if ($env:FAKE_GATE_EXIT -ne '0') { [Console]::Error.WriteLine('too complex: Branchy.Compute') }
exit [int]$env:FAKE_GATE_EXIT
'@
    }
    AfterEach { Remove-Item Env:FAKE_GATE_LOG, Env:FAKE_GATE_EXIT -ErrorAction SilentlyContinue }

    It 'GivenEditedCSharpFile_WhenGatePasses_ThenRunsGateOnceAndExitsZero' {
        # When
        $result = Invoke-Hook 'Edit' 'C:\repo\Code.cs'

        # Then
        $result.ExitCode | Should -Be 0
        @(Get-Content $script:Log) | Should -Be @('gate C:\repo')
    }

    It 'GivenEditedCSharpFile_WhenGateFails_ThenExitsTwoWithTheGateMessageOnce' {
        # Given
        $env:FAKE_GATE_EXIT = '2'

        # When
        $result = Invoke-Hook 'Edit' 'C:\repo\Code.cs'

        # Then
        $result.ExitCode | Should -Be 2
        ([regex]::Matches($result.Output, 'too complex: Branchy.Compute')).Count | Should -Be 1
    }

    It 'GivenWrittenCSharpFile_WhenGateFails_ThenExitsTwo' {
        # Given
        $env:FAKE_GATE_EXIT = '2'

        # When
        $result = Invoke-Hook 'Write' 'C:\repo\New.cs'

        # Then
        $result.ExitCode | Should -Be 2
    }

    It 'GivenEditedNonCSharpFile_WhenRun_ThenSkipsTheGate' {
        # When
        $result = Invoke-Hook 'Edit' 'C:\repo\README.md'

        # Then
        $result.ExitCode | Should -Be 0
        Test-Path $script:Log | Should -BeFalse
    }

    It 'GivenMalformedInput_WhenRun_ThenSkipsTheGateAndExitsZero' {
        # When
        $result = Invoke-HookRaw 'not json'

        # Then
        $result.ExitCode | Should -Be 0
        Test-Path $script:Log | Should -BeFalse
    }
}
