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

    It 'GivenEnvironmentSaysRobotClearToMove_WhenRun_ThenTestsSeeRobotNotClearToMove' {
        # Given
        $env:MAYDAY_ROBOT_IS_CLEAR_TO_MOVE = 'True'
        try {
            # When
            Invoke-Gate -Test 'param($c) Add-Content -Path ''LOG'' -Value "robot=$env:MAYDAY_ROBOT_IS_CLEAR_TO_MOVE"' | Out-Null
        }
        finally { Remove-Item Env:MAYDAY_ROBOT_IS_CLEAR_TO_MOVE }

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

    It 'GivenQuarantinedSwitch_WhenInspected_ThenItsRunExcludesPhysicalAndManualTiers' {
        # Given: the -Quarantined report runs every quarantined test in one process; with the flag set by the operator
        # (or by user secrets, which override the forced environment) physical tests in it would drive the robot
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:Gate, [ref]$null, [ref]$null)
        $block = $ast.FindAll({ param($node)
                $node -is [System.Management.Automation.Language.IfStatementAst] -and
                $node.Clauses[0].Item1.Extent.Text -ceq '$Quarantined' }, $true) | Select-Object -First 1

        # When
        $text = $block.Extent.Text

        # Then
        $text | Should -Match "--filter 'Quarantine=true&Tier!=Physical&Tier!=Manual'"
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

Describe 'gate.ps1 -Physical' {
    BeforeAll {
        # A scenario says what the fake test commands list and report, by filter. The fakes are scripts, so the gate's
        # child process can call them; they log every run, so a test can see which filters ran and in which order.
        $script:FakeList = Join-Path $TestDrive 'fake-list.ps1'
        Set-Content -Path $script:FakeList -Value @'
param($Configuration, $Filter, $Scenario)
$scenarioData = Get-Content -Path $Scenario -Raw | ConvertFrom-Json
@($scenarioData.lists.$Filter) | Where-Object { $_ }
$exit = $scenarioData.listExits.$Filter
if ($exit) { exit $exit }
'@
        $script:FakeRun = Join-Path $TestDrive 'fake-run.ps1'
        Set-Content -Path $script:FakeRun -Value @'
param($Configuration, $Filter, $ResultsFile, $Scenario, $Log)
Add-Content -Path $Log -Value "run $Filter robot=$env:MAYDAY_ROBOT_IS_CLEAR_TO_MOVE"
$results = @((Get-Content -Path $Scenario -Raw | ConvertFrom-Json).runs.$Filter) | Where-Object { $_ }
New-Item -ItemType Directory -Force -Path (Split-Path $ResultsFile) | Out-Null
$rows = $results | ForEach-Object { "<UnitTestResult testName=`"$($_.name)`" outcome=`"$($_.outcome)`" />" }
Set-Content -Path $ResultsFile -Value ('<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results>' +
    ($rows -join '') + '</Results></TestRun>')
$scenarioData = Get-Content -Path $Scenario -Raw | ConvertFrom-Json
if ($scenarioData.stderr) { cmd /c "echo [FAIL] $Filter 1>&2" }
$exit = $scenarioData.exits.$Filter
if ($exit) { exit $exit }
if ($results | Where-Object { $_.outcome -eq 'Failed' }) { exit 1 }
exit 0
'@

        $script:HardwareFree = 'Quarantine!=true&Tier!=Physical&Tier!=Manual'
        $script:QuarantinedHardwareFree = 'Quarantine=true&Tier!=Physical&Tier!=Manual'
        $script:PhysicalList = 'Tier=Physical&Quarantine!=true'
        $script:QuarantinedPhysicalList = 'Tier=Physical&Quarantine=true'

        function New-Result([string] $Name, [string] $Outcome = 'Passed') { @{ name = $Name; outcome = $Outcome } }

        # The robot's tests as they are today, all passing: one physical test, one quarantined physical theory that
        # fails, one hardware-free test and one quarantined hardware-free test that fails.
        function New-Scenario {
            @{
                lists = @{
                    $script:PhysicalList = @('T.Bus.Moves')
                    $script:QuarantinedPhysicalList = @('T.Leg.Transform(id: 0)', 'T.Leg.Transform(id: 1)')
                }
                runs = @{
                    $script:HardwareFree = @(New-Result 'T.Unit.Adds')
                    $script:QuarantinedHardwareFree = @(New-Result 'T.Unit.Broken' 'Failed')
                    'FullyQualifiedName=T.Bus.Moves' = @(New-Result 'T.Bus.Moves')
                    'FullyQualifiedName=T.Leg.Transform' = @(
                        (New-Result 'T.Leg.Transform(id: 0)' 'Failed'), (New-Result 'T.Leg.Transform(id: 1)'))
                }
            }
        }

        function Invoke-PhysicalGate(
            $Scenario = (New-Scenario),
            [string] $ClearToMove = "'True'",
            [string] $Connected = '$global:LASTEXITCODE = 0',
            [string] $Ratchet = '$global:LASTEXITCODE = 0',
            [string] $Prelude = '',
            [string] $Redirect = '') {
            $scenarioFile = Join-Path $TestDrive "scenario-$([guid]::NewGuid().ToString('N')).json"
            $Scenario | ConvertTo-Json -Depth 5 | Set-Content -Path $scenarioFile
            Invoke-Wrapper @"
$Prelude
& '$script:Gate' -RepoRoot '$script:Repo' -Physical ``
    -BuildCommand { param(`$c) Add-Content -Path '$script:Log' -Value "build `$c" } ``
    -ScriptTestCommand { } ``
    -RatchetCommand { $Ratchet } ``
    -ClearToMoveCommand { $ClearToMove } ``
    -ConnectedCommand { $Connected } ``
    -ListTestsCommand { param(`$c, `$f) & '$script:FakeList' `$c `$f '$scenarioFile' } ``
    -TestRunCommand { param(`$c, `$f, `$r) & '$script:FakeRun' `$c `$f `$r '$scenarioFile' '$script:Log' } $Redirect
exit `$LASTEXITCODE
"@
        }

        function Get-Runs { Get-Steps | Where-Object { $_ -like 'run *' } | ForEach-Object { ($_ -split ' ')[1] } }
    }

    BeforeEach {
        $script:Repo = Join-Path $TestDrive "repo-$([guid]::NewGuid().ToString('N'))"
        $script:Log = Join-Path $TestDrive "log-$([guid]::NewGuid().ToString('N')).txt"
        New-Item -ItemType Directory -Path $script:Repo | Out-Null
        git -C $script:Repo init -q
        Set-Content -Path (Join-Path $script:Repo 'Code.cs') -Value 'class A {}'
    }

    It 'GivenRatchetFails_WhenRun_ThenFailsBeforeBuildingOrTesting' {
        # When
        $result = Invoke-PhysicalGate -Ratchet 'Write-Host "Quarantine ratchet: X is now quarantined"; $global:LASTEXITCODE = 1'

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'X is now quarantined'
        Get-Steps | Should -BeNullOrEmpty
    }

    It 'GivenDefaultRatchetCommand_WhenInspected_ThenComparesWithOriginMaster' {
        # Given
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:Gate, [ref]$null, [ref]$null)
        $ratchet = $ast.ParamBlock.Parameters | Where-Object { $_.Name.VariablePath.UserPath -eq 'RatchetCommand' }

        # When
        $default = $ratchet.DefaultValue.Extent.Text

        # Then
        $default | Should -Match 'quarantine-ratchet\.ps1'
        $default | Should -Match '-Upstream origin/master'
    }

    It 'GivenRobotNotClearToMove_WhenRun_ThenFailsNamingTheFlagBeforeBuilding' {
        # When
        $result = Invoke-PhysicalGate -ClearToMove "'False'"

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match "MAYDAY_ROBOT_IS_CLEAR_TO_MOVE is 'False'"
        Get-Steps | Should -BeNullOrEmpty
    }

    It 'GivenClearToMoveFlagMissing_WhenRun_ThenFails' {
        # When
        $result = Invoke-PhysicalGate -ClearToMove '$null'

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'MAYDAY_ROBOT_IS_CLEAR_TO_MOVE'
    }

    It 'GivenRobotNotConnected_WhenRun_ThenFailsSayingSoBeforeBuilding' {
        # When
        $result = Invoke-PhysicalGate -Connected 'Write-Host "Robot not connected: no adapter"; $global:LASTEXITCODE = 1'

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Robot not connected'
        Get-Steps | Should -BeNullOrEmpty
    }

    It 'GivenBaselineHolds_WhenRun_ThenRunsHardwareFreeQuarantinedAndEachPhysicalMethodAlone' {
        # When
        $result = Invoke-PhysicalGate

        # Then: a theory listed once per case runs once, as its method
        $result.ExitCode | Should -Be 0
        Get-Runs | Should -Be @($script:HardwareFree, $script:QuarantinedHardwareFree,
            'FullyQualifiedName=T.Bus.Moves', 'FullyQualifiedName=T.Leg.Transform')
        (Get-Steps)[0] | Should -Be 'build Release'
    }

    It 'GivenNoPhysicalTestListed_WhenRun_ThenFailsInsteadOfPassingWithoutTheRobot' {
        # Given: a listing that matches nothing, as a changed test platform output format would give
        $scenario = New-Scenario
        $scenario.lists[$script:PhysicalList] = @()

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'No physical test is listed'
        Get-Runs | Should -BeNullOrEmpty
    }

    It 'GivenListingTheQuarantinedPhysicalTestsFails_WhenRun_ThenFails' {
        # Given
        $scenario = New-Scenario
        $scenario.listExits = @{ $script:QuarantinedPhysicalList = 3 }

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Listing the tests matching .* failed \(exit 3\)'
    }

    It 'GivenNoQuarantinedPhysicalTest_WhenRun_ThenPasses' {
        # Given: an empty quarantine is the goal, not a fault
        $scenario = New-Scenario
        $scenario.lists[$script:QuarantinedPhysicalList] = @()

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.ExitCode | Should -Be 0
    }

    It 'GivenPhysicalTestFails_WhenRun_ThenFailsNamingIt' {
        # Given
        $scenario = New-Scenario
        $scenario.runs['FullyQualifiedName=T.Bus.Moves'] = @(New-Result 'T.Bus.Moves' 'Failed')

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'FAILED T\.Bus\.Moves'
    }

    It 'GivenHardwareFreeTestFails_WhenRun_ThenStillRunsThePhysicalTestsAndReportsAll' {
        # Given
        $scenario = New-Scenario
        $scenario.runs[$script:HardwareFree] = @(New-Result 'T.Unit.Adds' 'Failed')

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'FAILED T\.Unit\.Adds'
        Get-Runs | Should -Contain 'FullyQualifiedName=T.Bus.Moves'
    }

    It 'GivenSkippedTest_WhenRun_ThenFailsListingIt' {
        # Given
        $scenario = New-Scenario
        $scenario.runs[$script:HardwareFree] = @((New-Result 'T.Unit.Adds'), (New-Result 'T.Unit.Later' 'NotExecuted'))

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'SKIPPED T\.Unit\.Later'
    }

    It 'GivenSkippedQuarantinedTest_WhenRun_ThenFailsListingIt' {
        # Given
        $scenario = New-Scenario
        $scenario.runs[$script:QuarantinedHardwareFree] = @(New-Result 'T.Unit.Broken' 'NotExecuted')

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.Output | Should -Match 'SKIPPED T\.Unit\.Broken'
        $result.ExitCode | Should -Not -Be 0
    }

    It 'GivenQuarantinedTestPasses_WhenRun_ThenFailsAskingToRemoveItsQuarantine' {
        # Given
        $scenario = New-Scenario
        $scenario.runs['FullyQualifiedName=T.Leg.Transform'] = @(
            (New-Result 'T.Leg.Transform(id: 0)'), (New-Result 'T.Leg.Transform(id: 1)'))

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'PASSES WHILE QUARANTINED T\.Leg\.Transform: remove its \[Quarantine\]'
    }

    It 'GivenPhysicalMethodRunMatchesNoTest_WhenRun_ThenFailsInsteadOfPassingWithoutTheRobot' {
        # Given: the listed name no longer matches the test's fully qualified name, so the run executes nothing,
        # exits 0 and writes an empty TRX file
        $scenario = New-Scenario
        $scenario.runs['FullyQualifiedName=T.Bus.Moves'] = @()

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Physical test T\.Bus\.Moves: no test executed'
    }

    It 'GivenQuarantinedPhysicalMethodRunMatchesNoTest_WhenRun_ThenFails' {
        # Given
        $scenario = New-Scenario
        $scenario.runs['FullyQualifiedName=T.Leg.Transform'] = @()

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Quarantined physical test T\.Leg\.Transform: no test executed'
    }

    It 'GivenTestHostCrashesWithoutReportingAFailure_WhenRun_ThenFails' {
        # Given
        $scenario = New-Scenario
        $scenario.runs['FullyQualifiedName=T.Bus.Moves'] = @()
        $scenario.exits = @{ 'FullyQualifiedName=T.Bus.Moves' = 2 }

        # When
        $result = Invoke-PhysicalGate $scenario

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'exit 2 with no failing test reported'
    }

    It 'GivenFailingTestsWriteToStderr_WhenRunWithRedirectedStreams_ThenStillJudgesAllRuns' {
        # Given: dotnet test reports a failing test on stderr, and the caller redirects the gate's streams
        $scenario = New-Scenario
        $scenario.stderr = $true

        # When
        $result = Invoke-PhysicalGate $scenario -Redirect '*>&1 | Out-Host'

        # Then: the quarantined failure is expected, so the baseline still holds
        $result.ExitCode | Should -Be 0
        Get-Runs | Should -HaveCount 4
    }

    It 'GivenPhysicalPassRemembered_WhenTheRatchetNowFailsOnTheUnchangedTree_ThenFailsAnyway' {
        # Given: an uncommitted quarantine passes the ratchet, which reads commits only, and the pass is remembered
        Invoke-PhysicalGate | Out-Null

        # When: the quarantine is committed, which changes no file in the working tree
        $result = Invoke-PhysicalGate -Ratchet 'Write-Host "Quarantine ratchet: X is now quarantined"; $global:LASTEXITCODE = 1'

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'X is now quarantined'
    }

    It 'GivenPhysicalPassOnUnchangedTree_WhenRunAgain_ThenDoesNotDriveTheRobotAgain' {
        # Given
        Invoke-PhysicalGate | Out-Null

        # When
        $result = Invoke-PhysicalGate

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'already passed'
        Get-Runs | Should -HaveCount 4
    }

    It 'GivenHardwareFreePassOnUnchangedTree_WhenRunPhysical_ThenRunsBecauseThePassesAreKeptApart' {
        # Given
        Invoke-Gate | Out-Null

        # When
        Invoke-PhysicalGate | Out-Null

        # Then
        Get-Runs | Should -HaveCount 4
    }

    It 'GivenOperatorSetClearToMoveInEnvironment_WhenRun_ThenTestsSeeItUnchanged' {
        # When
        Invoke-PhysicalGate -Prelude '$env:MAYDAY_ROBOT_IS_CLEAR_TO_MOVE = ''True''' | Out-Null

        # Then
        Get-Steps | Where-Object { $_ -like 'run *' } | ForEach-Object { $_ | Should -Match 'robot=True$' }
    }

    It 'GivenRunWithoutPhysical_WhenRun_ThenNeverListsOrRunsPhysicalTests' {
        # When
        $result = Invoke-Gate

        # Then: the hardware-free gate still uses its own test command only
        $result.ExitCode | Should -Be 0
        Get-Runs | Should -BeNullOrEmpty
    }
}

Describe 'gate.ps1 default clear-to-move check' {
    BeforeAll {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:Gate, [ref]$null, [ref]$null)
        $parameter = $ast.ParamBlock.Parameters | Where-Object { $_.Name.VariablePath.UserPath -eq 'ClearToMoveCommand' }
        $script:ClearToMove = $parameter.DefaultValue.ScriptBlock.GetScriptBlock()
    }

    BeforeEach {
        $script:Root = Join-Path $TestDrive "root-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Force -Path (Join-Path $script:Root 'Test') | Out-Null
        Set-Content -Path (Join-Path $script:Root 'Test/Test.csproj') -Value '<Project><PropertyGroup><UserSecretsId>abc-123</UserSecretsId></PropertyGroup></Project>'
        $script:OldAppData = $env:APPDATA
        $env:APPDATA = Join-Path $TestDrive "appdata-$([guid]::NewGuid().ToString('N'))"
        $script:Secrets = Join-Path $env:APPDATA 'Microsoft/UserSecrets/abc-123/secrets.json'
        New-Item -ItemType Directory -Force -Path (Split-Path $script:Secrets) | Out-Null
    }

    AfterEach {
        $env:APPDATA = $script:OldAppData
        Remove-Item Env:MAYDAY_ROBOT_IS_CLEAR_TO_MOVE -ErrorAction SilentlyContinue
    }

    It 'GivenSecretIsJsonTrueWithTrailingComma_WhenChecked_ThenReturnsTrue' {
        # Given: the configuration reader accepts trailing commas, and the owner's secrets file has one
        Set-Content -Path $script:Secrets -Value "{`n  `"MAYDAY_ROBOT_IS_CLEAR_TO_MOVE`" : true,`n  `"MAYDAY_HUMAN_IS_PRESENT`" : false,`n}"

        # When / Then
        & $script:ClearToMove $script:Root | Should -BeExactly 'True'
    }

    It 'GivenSecretIsFalseAndEnvironmentTrue_WhenChecked_ThenTheSecretWinsAsInTheTests' {
        # Given
        Set-Content -Path $script:Secrets -Value '{ "MAYDAY_ROBOT_IS_CLEAR_TO_MOVE": false }'
        $env:MAYDAY_ROBOT_IS_CLEAR_TO_MOVE = 'True'

        # When / Then
        & $script:ClearToMove $script:Root | Should -BeExactly 'False'
    }

    It 'GivenNoSecretAndEnvironmentTrue_WhenChecked_ThenReturnsTheEnvironment' {
        # Given
        Set-Content -Path $script:Secrets -Value '{ "MAYDAY_HUMAN_IS_PRESENT": false }'
        $env:MAYDAY_ROBOT_IS_CLEAR_TO_MOVE = 'True'

        # When / Then
        & $script:ClearToMove $script:Root | Should -BeExactly 'True'
    }

    It 'GivenOnlyTheOldConnectedKey_WhenChecked_ThenReturnsNothing' {
        # Given
        Set-Content -Path $script:Secrets -Value '{ "MAYDAY_ROBOT_IS_CONNECTED": true }'

        # When / Then
        & $script:ClearToMove $script:Root | Should -BeNullOrEmpty
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

    It 'GivenStopHook_WhenRuns_ThenNeverAsksForThePhysicalTier' {
        # Given: the robot is driven only on push, never on a Claude Code stop
        Set-Content -Path $script:FakeGate -Value 'param($Configuration, [switch] $Physical); if ($Physical) { exit 1 } else { exit 0 }'

        # When
        $result = Invoke-Wrapper "'' | & '$script:StopGate' -GateScript '$script:FakeGate'; exit `$LASTEXITCODE"

        # Then
        $result.ExitCode | Should -Be 0
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
