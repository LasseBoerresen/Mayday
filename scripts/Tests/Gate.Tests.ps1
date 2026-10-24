#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
# Tests for scripts/gate.ps1 and scripts/claude-stop-gate.ps1.
#
# Each test gets a scratch git repository and runs the scripts in a child PowerShell, because the scripts call
# `exit`. The build and test steps are replaced with commands that append to a log, so nothing compiles and the
# log shows which steps ran, in order. Names follow the repo's Given/When/Then convention.

BeforeAll {
    $script:Gate = Join-Path $PSScriptRoot '..\gate.ps1' | Resolve-Path | Select-Object -ExpandProperty Path
    $script:StopGate = Join-Path $PSScriptRoot '..\claude-stop-gate.ps1' | Resolve-Path | Select-Object -ExpandProperty Path

    # Runs powershell on a wrapper script and returns its exit code and combined output.
    function Invoke-Wrapper([string] $Body) {
        $wrapper = Join-Path $TestDrive "wrapper-$([guid]::NewGuid().ToString('N')).ps1"
        Set-Content -Path $wrapper -Value $Body
        # The gate runs Pester with ErrorActionPreference=Stop, which would turn the child's stderr into an exception.
        $ErrorActionPreference = 'Continue'
        $output = '' | & powershell -NoProfile -ExecutionPolicy Bypass -File $wrapper 2>&1 | ForEach-Object { "$_" } | Out-String
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output }
    }

    # Runs the gate against $script:Repo. Build and Test are script block bodies; LOG is replaced by the log path.
    function Invoke-Gate(
        [string] $Configuration = 'Release',
        [string] $Build = 'param($c) Add-Content -Path ''LOG'' -Value "build $c"',
        [string] $Test = 'param($c) Add-Content -Path ''LOG'' -Value "test $c"',
        [switch] $Force) {
        $forceArg = if ($Force) { '-Force' } else { '' }
        $body = @"
& '$script:Gate' -RepoRoot '$script:Repo' -Configuration $Configuration $forceArg ``
    -BuildCommand { $Build } -TestCommand { $Test } -ScriptTestCommand { }
exit `$LASTEXITCODE
"@.Replace('LOG', $script:Log)
        Invoke-Wrapper $body
    }

    function Get-Steps { if (Test-Path $script:Log) { @(Get-Content $script:Log) } else { @() } }

    # Runs the Stop hook as Claude Code does, as its own process, and returns its exit code and raw stderr. The
    # stderr goes straight to a file, so it holds exactly what Claude Code would read, without PowerShell wrapping.
    function Invoke-StopHook([string] $GateScript) {
        $stdin = Join-Path $TestDrive "stdin-$([guid]::NewGuid().ToString('N')).txt"
        $stdout = Join-Path $TestDrive "stdout-$([guid]::NewGuid().ToString('N')).txt"
        $stderr = Join-Path $TestDrive "stderr-$([guid]::NewGuid().ToString('N')).txt"
        Set-Content -Path $stdin -Value ''
        $process = Start-Process -FilePath powershell -NoNewWindow -Wait -PassThru `
            -ArgumentList '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', "`"$script:StopGate`"", '-GateScript', "`"$GateScript`"" `
            -RedirectStandardInput $stdin -RedirectStandardOutput $stdout -RedirectStandardError $stderr
        [pscustomobject]@{ ExitCode = $process.ExitCode; Stderr = (Get-Content -Path $stderr -Raw) }
    }
}

Describe 'gate.ps1' {
    BeforeEach {
        $script:Repo = Join-Path $TestDrive "repo-$([guid]::NewGuid().ToString('N'))"
        $script:Log = Join-Path $TestDrive "log-$([guid]::NewGuid().ToString('N')).txt"
        New-Item -ItemType Directory -Path $script:Repo | Out-Null
        git -C $script:Repo init -q
        Set-Content -Path (Join-Path $script:Repo 'Code.cs') -Value 'class A {}'
        Set-Content -Path (Join-Path $script:Repo 'Notes.md') -Value 'notes'
    }

    It 'GivenGitHookEnvironmentForAnotherRepo_WhenRun_ThenGatesRepoRootAndHidesItFromSteps' {
        # Given: git runs hooks of a linked worktree with GIT_DIR set to an absolute path, here another repository
        $other = Join-Path $TestDrive "other-$([guid]::NewGuid().ToString('N'))"
        git init -q $other
        $otherGitDir = Join-Path $other '.git'
        $test = 'param($c) Add-Content -Path ''LOG'' -Value "git_dir=$env:GIT_DIR"'.Replace('LOG', $script:Log)

        # When
        $result = Invoke-Wrapper @"
`$env:GIT_DIR = '$otherGitDir'
& '$script:Gate' -RepoRoot '$script:Repo' -BuildCommand { } -TestCommand { $test } -ScriptTestCommand { }
exit `$LASTEXITCODE
"@

        # Then: the gate remembers its pass in RepoRoot's repository, and steps (which create scratch repositories
        # of their own) do not inherit a GIT_DIR that would point their git commands at another repository
        $result.ExitCode | Should -Be 0
        Join-Path $script:Repo '.git/gate-passed-Release' | Should -Exist
        Join-Path $otherGitDir 'gate-passed-Release' | Should -Not -Exist
        Get-Steps | Should -Be @('git_dir=')
    }

    It 'GivenPassingBuildAndTests_WhenRun_ThenExitsZeroAndRunsBuildBeforeTests' {
        # When
        $result = Invoke-Gate

        # Then
        $result.ExitCode | Should -Be 0
        Get-Steps | Should -Be @('build Release', 'test Release')
    }

    It 'GivenGatePassedOnUnchangedTree_WhenRunAgain_ThenSkipsBuildAndTests' {
        # Given
        Invoke-Gate | Out-Null

        # When
        $result = Invoke-Gate

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'already passed'
        Get-Steps | Should -Be @('build Release', 'test Release')
    }

    It 'GivenGatePassedOnUnchangedTree_WhenForced_ThenRunsAgain' {
        # Given
        Invoke-Gate | Out-Null

        # When
        Invoke-Gate -Force | Out-Null

        # Then
        Get-Steps | Should -HaveCount 4
    }

    It 'GivenGatePassed_WhenCodeFileChanges_ThenRunsAgain' {
        # Given
        Invoke-Gate | Out-Null
        Add-Content -Path (Join-Path $script:Repo 'Code.cs') -Value 'class B {}'

        # When
        Invoke-Gate | Out-Null

        # Then
        Get-Steps | Should -HaveCount 4
    }

    It 'GivenGatePassed_WhenNewUntrackedCodeFileAppears_ThenRunsAgain' {
        # Given
        Invoke-Gate | Out-Null
        Set-Content -Path (Join-Path $script:Repo 'New.cs') -Value 'class C {}'

        # When
        Invoke-Gate | Out-Null

        # Then
        Get-Steps | Should -HaveCount 4
    }

    It 'GivenGatePassed_WhenOnlyMarkdownChanges_ThenSkips' {
        # Given
        Invoke-Gate | Out-Null
        Add-Content -Path (Join-Path $script:Repo 'Notes.md') -Value 'more notes'

        # When
        $result = Invoke-Gate

        # Then
        $result.Output | Should -Match 'already passed'
        Get-Steps | Should -HaveCount 2
    }

    It 'GivenGatePassedInDebug_WhenRunInRelease_ThenRunsBecausePassesAreRememberedPerConfiguration' {
        # Given
        Invoke-Gate -Configuration Debug | Out-Null

        # When
        Invoke-Gate -Configuration Release | Out-Null

        # Then
        Get-Steps | Should -Be @('build Debug', 'test Debug', 'build Release', 'test Release')
    }

    It 'GivenFailingBuild_WhenRun_ThenExitsWithItsCodeAndSkipsTests' {
        # When
        $result = Invoke-Gate -Build 'param($c) Add-Content -Path ''LOG'' -Value "build"; cmd /c exit 3'

        # Then
        $result.ExitCode | Should -Be 3
        Get-Steps | Should -Be @('build')
    }

    It 'GivenFailingTests_WhenRun_ThenExitsNonZero' {
        # When
        $result = Invoke-Gate -Test 'param($c) cmd /c exit 1'

        # Then
        $result.ExitCode | Should -Be 1
        $result.Output | Should -Match 'FAILED'
    }

    It 'GivenFailedRun_WhenRerunOnUnchangedTree_ThenDoesNotSkip' {
        # Given
        Invoke-Gate -Test 'param($c) cmd /c exit 1' | Out-Null

        # When
        $result = Invoke-Gate

        # Then
        $result.ExitCode | Should -Be 0
        Get-Steps | Should -Be @('build Release', 'build Release', 'test Release')
    }

    It 'GivenEnvironmentSaysRobotConnected_WhenRun_ThenTestsSeeRobotDisconnected' {
        # Given
        $env:MAYDAY_ROBOT_IS_CONNECTED = 'True'
        try {
            # When
            Invoke-Gate -Test 'param($c) Add-Content -Path ''LOG'' -Value "robot=$env:MAYDAY_ROBOT_IS_CONNECTED"' | Out-Null
        }
        finally { Remove-Item Env:MAYDAY_ROBOT_IS_CONNECTED }

        # Then
        Get-Steps | Should -Contain 'robot=False'
    }

    It 'GivenDefaultTestCommand_WhenInspected_ThenExcludesPhysicalAndManualTiers' {
        # Given: the user secrets can say the robot is connected, which overrides the forced environment variable,
        # so the filter is what keeps the gate off the robot. The default is a parameter value, read from the script.
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:Gate, [ref]$null, [ref]$null)
        $testCommand = $ast.ParamBlock.Parameters | Where-Object { $_.Name.VariablePath.UserPath -eq 'TestCommand' }

        # When
        $default = $testCommand.DefaultValue.Extent.Text

        # Then
        $default | Should -Match 'Tier!=Physical'
        $default | Should -Match 'Tier!=Manual'
        $default | Should -Match 'Quarantine!=true'
    }

    It 'GivenFailingScriptTests_WhenRun_ThenExitsNonZero' {
        # Given
        $body = @"
& '$script:Gate' -RepoRoot '$script:Repo' -BuildCommand { } -TestCommand { } -ScriptTestCommand { cmd /c exit 1 }
exit `$LASTEXITCODE
"@
        # When
        $result = Invoke-Wrapper $body

        # Then
        $result.ExitCode | Should -Be 1
    }
}

Describe 'claude-stop-gate.ps1' {
    BeforeEach {
        $script:FakeGate = Join-Path $TestDrive "fake-gate-$([guid]::NewGuid().ToString('N')).ps1"
    }

    It 'GivenGatePasses_WhenStopHookRuns_ThenExitsZero' {
        # Given
        Set-Content -Path $script:FakeGate -Value 'param($Configuration); exit 0'

        # When
        $result = Invoke-Wrapper "'' | & '$script:StopGate' -GateScript '$script:FakeGate'; exit `$LASTEXITCODE"

        # Then
        $result.ExitCode | Should -Be 0
    }

    It 'GivenGateFails_WhenStopHookRuns_ThenExitsTwoAndReportsGateOutput' {
        # Given
        Set-Content -Path $script:FakeGate -Value 'param($Configuration); Write-Host "boom in tests"; exit 1'

        # When
        $result = Invoke-Wrapper "'' | & '$script:StopGate' -GateScript '$script:FakeGate'; exit `$LASTEXITCODE"

        # Then
        $result.ExitCode | Should -Be 2
        $result.Output | Should -Match 'Local gate failed'
        $result.Output | Should -Match 'boom in tests'
    }

    It 'GivenGateFailsWithHostOutput_WhenStopHookRuns_ThenReportsThatOutputOnStderr' {
        # Given: the gate reports its steps with Write-Host, and Claude Code reads only the hook's stderr
        Set-Content -Path $script:FakeGate -Value 'param($Configuration); Write-Host "== FAILED: boom step"; exit 1'
        $stderr = Join-Path $TestDrive "stderr-$([guid]::NewGuid().ToString('N')).txt"

        # When
        $ErrorActionPreference = 'Continue'
        '' | & powershell -NoProfile -ExecutionPolicy Bypass -File $script:StopGate -GateScript $script:FakeGate 2> $stderr | Out-Null
        $exitCode = $LASTEXITCODE

        # Then
        $exitCode | Should -Be 2
        Get-Content -Path $stderr -Raw | Should -Match '== FAILED: boom step'
    }

    It 'GivenGatePassesWithNativeStderr_WhenStopHookRuns_ThenExitsZero' {
        # Given: like gate.ps1, the gate runs with ErrorActionPreference=Stop and a tool it calls writes to stderr
        Set-Content -Path $script:FakeGate -Value 'param($Configuration); $ErrorActionPreference = ''Stop''; cmd /c "echo tool warning 1>&2"; exit 0'

        # When
        $result = Invoke-StopHook $script:FakeGate

        # Then
        $result.ExitCode | Should -Be 0
    }

    It 'GivenGateFailsWithNativeStderr_WhenStopHookRuns_ThenExitsTwoAndReportsStderrLinesPlainly' {
        # Given
        Set-Content -Path $script:FakeGate -Value 'param($Configuration); $ErrorActionPreference = ''Stop''; cmd /c "echo boom on stderr 1>&2"; exit 1'

        # When
        $result = Invoke-StopHook $script:FakeGate

        # Then: the line arrives as written, not as a five-line PowerShell error record
        $result.ExitCode | Should -Be 2
        $result.Stderr | Should -Match 'boom on stderr'
        $result.Stderr | Should -Not -Match 'NativeCommandError'
    }

    It 'GivenGateThrows_WhenStopHookRuns_ThenExitsTwoAndReportsTheError' {
        # Given
        Set-Content -Path $script:FakeGate -Value 'param($Configuration); throw ''gate crashed'''

        # When
        $result = Invoke-StopHook $script:FakeGate

        # Then: a crashed gate blocks the turn like a failed one; exit 1 would let Claude Code carry on
        $result.ExitCode | Should -Be 2
        $result.Stderr | Should -Match 'gate crashed'
    }

    It 'GivenGateRunsInDebug_WhenStopHookRuns_ThenPassesDebugConfiguration' {
        # Given
        Set-Content -Path $script:FakeGate -Value 'param($Configuration); if ($Configuration -eq ''Debug'') { exit 0 } else { exit 1 }'

        # When
        $result = Invoke-Wrapper "'' | & '$script:StopGate' -GateScript '$script:FakeGate'; exit `$LASTEXITCODE"

        # Then
        $result.ExitCode | Should -Be 0
    }
}
