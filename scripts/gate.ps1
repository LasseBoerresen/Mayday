<#
.SYNOPSIS
    Runs the local quality gate: build the solution, then run the blocking test set.

.DESCRIPTION
    The same checks as the optional GitHub workflow (.github/workflows/ci.yml): a build and the tests that are
    not marked [Quarantine]; plus the Pester tests of these scripts. Exits non-zero when any step fails.

    A pass is remembered per configuration against a fingerprint of the working tree (every non-Markdown file
    git tracks or could track). When the tree is unchanged since the last pass the gate returns immediately, so
    it can run on every Claude Code stop and every push without repeating work. Use -Force to ignore the memory.

    Without -Physical the gate never drives the robot: the tests marked Tier=Physical or Tier=Manual are excluded,
    because user secrets can say the robot is clear to move and override the environment.
    MAYDAY_ROBOT_IS_CLEAR_TO_MOVE is also forced to False, so a physical test that lacks the trait still skips. This is
    the Claude Code Stop hook's gate.

    With -Physical (the pre-push gate) it runs every test except Tier=Manual against the robot. It first fails
    unless the operator has said the robot is clear to move (MAYDAY_ROBOT_IS_CLEAR_TO_MOVE is True, read like the
    tests read it) and the robot is connected (its serial adapter is present and its port opens). Then it runs the
    hardware-free tests, the quarantined hardware-free tests and every physical test method in a process of its own,
    so a test that leaves the robot's port open cannot fail the next one. It fails when a test that is not quarantined
    fails, when any test is skipped, and when a quarantined test passes: its [Quarantine] must then be removed. It
    reports every such test before failing. Main and EllieMain are never run; manual tests are run by hand.

.PARAMETER Configuration
    Build configuration. Release matches CI; Debug is faster for tight loops.

.PARAMETER Quarantined
    Also run the [Quarantine] tests, non-blocking, and report any that now pass.

.PARAMETER Physical
    Run the physical tier against the robot as described above. A physical pass is remembered apart from a
    hardware-free one.

.PARAMETER Force
    Run even when the working tree matches the last passing run.

.PARAMETER RepoRoot
    The git working tree to gate. Defaults to the repository this script lives in; the script tests point it at a
    scratch repository.

.PARAMETER BuildCommand
    Runs the build; receives the configuration. Must leave a non-zero $LASTEXITCODE on failure. Defaults to
    building Mayday.sln. Overridden by the script tests so they do not compile the solution.

.PARAMETER TestCommand
    Runs the blocking tests, with the same contract as -BuildCommand. Defaults to the Test project without the
    [Quarantine], physical and manual tests.

.PARAMETER ScriptTestCommand
    Runs the tests of the scripts themselves, with the same contract as -BuildCommand. Defaults to the Pester
    tests in scripts/Tests. The script tests replace it with a no-op so the gate does not run itself recursively.

.PARAMETER ClearToMoveCommand
    With -Physical: receives the repository root and returns the value of MAYDAY_ROBOT_IS_CLEAR_TO_MOVE. Defaults to
    the Test project's user secrets, falling back to the environment, the precedence the tests use.

.PARAMETER ConnectedCommand
    With -Physical: checks that the robot is connected, leaving a non-zero $LASTEXITCODE when it is not. Defaults to
    finding Mayday's FTDI serial adapter and opening its port.

.PARAMETER ListTestsCommand
    With -Physical: receives the configuration and a test filter and returns the fully qualified names of the
    matching tests. Defaults to dotnet test --list-tests on the Test project.

.PARAMETER TestRunCommand
    With -Physical: receives the configuration, a test filter and a results file path, runs the matching tests and
    writes their results there as TRX. Defaults to dotnet test on the Test project.
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',
    [switch] $Quarantined,
    [switch] $Physical,
    [switch] $Force,
    [string] $RepoRoot,
    [scriptblock] $BuildCommand = {
        param($Configuration)
        dotnet build Mayday.sln --configuration $Configuration --nologo --verbosity quiet
    },
    [scriptblock] $TestCommand = {
        param($Configuration)
        dotnet test Test/Test.csproj --configuration $Configuration --no-build --nologo `
            --filter 'Quarantine!=true&Tier!=Physical&Tier!=Manual'
    },
    [scriptblock] $ScriptTestCommand = {
        param($Configuration)
        try { Import-Module Pester -MinimumVersion 5.0 -ErrorAction Stop }
        catch {
            Write-Host 'Pester 5+ is required: Install-Module Pester -MinimumVersion 5.0 -Scope CurrentUser'
            $global:LASTEXITCODE = 1
            return
        }
        $result = Invoke-Pester -Path 'scripts/Tests', 'quality-gates/Tests' -Output Minimal -PassThru
        if ($result.Result -ne 'Passed') { $global:LASTEXITCODE = 1 }
    },
    [scriptblock] $ClearToMoveCommand = {
        param($RepoRoot)
        # TestConfiguration adds the environment first and the user secrets second, so a secret wins.
        $value = $env:MAYDAY_ROBOT_IS_CLEAR_TO_MOVE
        $secretsId = Select-String -Path (Join-Path $RepoRoot 'Test/Test.csproj') -Pattern '<UserSecretsId>([^<]+)<' |
            ForEach-Object { $_.Matches[0].Groups[1].Value } | Select-Object -First 1
        $secrets = Join-Path $env:APPDATA "Microsoft/UserSecrets/$secretsId/secrets.json"
        # Read by pattern, not ConvertFrom-Json: the configuration reader accepts trailing commas, and so must this.
        if ($secretsId -and (Test-Path $secrets)) {
            $match = Select-String -Path $secrets -Pattern '"MAYDAY_ROBOT_IS_CLEAR_TO_MOVE"\s*:\s*"?([^",\s}]+)'
            if ($match) {
                $raw = $match.Matches[0].Groups[1].Value
                # A JSON true reaches the tests as "True".
                $value = if ($raw -eq 'true') { 'True' } elseif ($raw -eq 'false') { 'False' } else { $raw }
            }
        }
        $value
    },
    [scriptblock] $ConnectedCommand = {
        # The serial number Dynamixel's NativeSerialPortCommunicationBus opens the robot's adapter by.
        $serialNumber = 'FT2H2YMW'
        $adapter = Get-CimInstance Win32_PnPEntity | Where-Object { $_.DeviceID -like "FTDIBUS\*+$serialNumber*" } |
            Select-Object -First 1
        if (-not $adapter -or $adapter.Name -notmatch '\((COM\d+)\)') {
            Write-Host "Robot not connected: no FTDI serial adapter with serial number $serialNumber is present."
            $global:LASTEXITCODE = 1
            return
        }
        $portName = $Matches[1]
        $port = New-Object System.IO.Ports.SerialPort $portName
        try {
            $port.Open()
            $global:LASTEXITCODE = 0
        }
        catch {
            Write-Host "Robot not connected: port $portName of adapter $serialNumber does not open: $($_.Exception.Message)"
            $global:LASTEXITCODE = 1
        }
        finally { $port.Dispose() }
    },
    [scriptblock] $ListTestsCommand = {
        param($Configuration, $Filter)
        # dotnet test lists one test per line, indented by four spaces, after a header.
        dotnet test Test/Test.csproj --configuration $Configuration --no-build --nologo --list-tests --filter $Filter |
            Where-Object { $_ -match '^    \S' } | ForEach-Object { $_.Trim() }
    },
    [scriptblock] $TestRunCommand = {
        param($Configuration, $Filter, $ResultsFile)
        dotnet test Test/Test.csproj --configuration $Configuration --no-build --nologo --filter $Filter `
            --logger "trx;LogFileName=$(Split-Path -Leaf $ResultsFile)" --results-directory (Split-Path $ResultsFile)
    }
)

$ErrorActionPreference = 'Stop'
if (-not $RepoRoot) { $RepoRoot = Split-Path -Parent $PSScriptRoot }
Set-Location $RepoRoot

# git runs hooks of a linked worktree with GIT_DIR (and for some hooks GIT_INDEX_FILE) set to absolute paths. Left
# in place they override the repository found from $RepoRoot, here and in every step: the script tests' scratch
# `git init` would then reinitialize the real repository as bare. The gate finds its repository from $RepoRoot only.
'GIT_DIR', 'GIT_WORK_TREE', 'GIT_INDEX_FILE', 'GIT_COMMON_DIR', 'GIT_PREFIX', 'GIT_OBJECT_DIRECTORY',
    'GIT_ALTERNATE_OBJECT_DIRECTORIES' | ForEach-Object { Remove-Item -Path "Env:$_" -ErrorAction SilentlyContinue }

if (-not $Physical) { $env:MAYDAY_ROBOT_IS_CLEAR_TO_MOVE = 'False' }

function Get-TreeFingerprint {
    $files = git ls-files --cached --others --exclude-standard |
        Where-Object { $_ -notmatch '\.md$' -and $_ -notmatch '^(\.idea|\.claude|knowledge|docs|Media)/' } |
        Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } |
        Sort-Object
    $lines = foreach ($file in $files) { "$file $((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash)" }
    $bytes = [Text.Encoding]::UTF8.GetBytes(($lines -join "`n"))
    [BitConverter]::ToString([Security.Cryptography.SHA256]::Create().ComputeHash($bytes))
}

function Invoke-Step([string] $Name, [scriptblock] $Command) {
    Write-Host "== $Name"
    $global:LASTEXITCODE = 0
    & $Command
    if ($LASTEXITCODE -ne 0) {
        Write-Host "== FAILED: $Name (exit $LASTEXITCODE)"
        exit $LASTEXITCODE
    }
}

# The test runs of the physical tier: the hardware-free tests, the quarantined ones, then each physical test method
# in its own process. One process per method because a physical test that leaves the robot's port open makes every
# later one in the same process fail (MAY-81).
function Get-PhysicalTierRuns {
    [pscustomobject]@{ Name = 'Hardware-free tests'; Filter = 'Quarantine!=true&Tier!=Physical&Tier!=Manual'; Quarantined = $false }
    [pscustomobject]@{ Name = 'Quarantined hardware-free tests'; Filter = 'Quarantine=true&Tier!=Physical&Tier!=Manual'; Quarantined = $true }
    foreach ($quarantined in $false, $true) {
        $filter = 'Tier=Physical&Quarantine' + $(if ($quarantined) { '=' } else { '!=' }) + 'true'
        # A theory may be listed once per case; its cases run together, by method.
        $methods = & $ListTestsCommand $Configuration $filter | ForEach-Object { ($_ -split '\(')[0] } | Sort-Object -Unique
        foreach ($method in $methods) {
            $name = if ($quarantined) { "Quarantined physical test $method" } else { "Physical test $method" }
            [pscustomobject]@{ Name = $name; Filter = "FullyQualifiedName=$method"; Quarantined = $quarantined }
        }
    }
}

# What a test run's results break: a failing or skipped test, or, in a quarantined run, a test that passes.
function Get-RunProblems($Run, [string] $ResultsFile, [int] $ExitCode) {
    if (-not (Test-Path $ResultsFile)) {
        if ($ExitCode -ne 0) { "$($Run.Name): no results (exit $ExitCode)" }
        return
    }
    $results = @(([xml](Get-Content -Path $ResultsFile -Raw)).TestRun.Results.UnitTestResult | Where-Object { $_ })
    $byMethod = $results | Group-Object { ($_.testName -split '\(')[0] }
    foreach ($method in $byMethod) {
        $outcomes = @($method.Group | ForEach-Object { $_.outcome })
        if ($outcomes -contains 'NotExecuted') { "SKIPPED $($method.Name)" }
        elseif ($Run.Quarantined -and -not ($outcomes | Where-Object { $_ -ne 'Passed' })) {
            "PASSES WHILE QUARANTINED $($method.Name): remove its [Quarantine]"
        }
        elseif (-not $Run.Quarantined -and ($outcomes | Where-Object { $_ -ne 'Passed' })) { "FAILED $($method.Name)" }
    }
    if (-not $Run.Quarantined -and $ExitCode -ne 0 -and -not ($results | Where-Object { $_.outcome -ne 'Passed' })) {
        "$($Run.Name): exit $ExitCode with no failing test reported"
    }
}

$gitDir = git rev-parse --absolute-git-dir
$memory = Join-Path $gitDir ("gate-passed-$Configuration" + $(if ($Physical) { '-Physical' } else { '' }))
$fingerprint = Get-TreeFingerprint

if (-not $Force -and (Test-Path $memory) -and ((Get-Content $memory -Raw).Trim() -eq $fingerprint)) {
    Write-Host "== Gate ($Configuration) already passed for this exact working tree; skipping. Use -Force to rerun."
    exit 0
}

if ($Physical) {
    Invoke-Step 'Robot clear to move' {
        $clearToMove = & $ClearToMoveCommand $RepoRoot
        if ($clearToMove -cne 'True') {
            Write-Host ("MAYDAY_ROBOT_IS_CLEAR_TO_MOVE is '$clearToMove', not 'True', in the Test user secrets or the " +
                'environment. Set it to True yourself once Mayday is placed safely and may move.')
            $global:LASTEXITCODE = 1
        }
    }
    Invoke-Step 'Robot connected' { & $ConnectedCommand }
}

Invoke-Step "Build ($Configuration)" { & $BuildCommand $Configuration }

if ($Physical) {
    Invoke-Step "Tests with the robot, manual excluded ($Configuration)" {
        $resultsDir = Join-Path $gitDir 'gate-results'
        Remove-Item -Recurse -Force -Path $resultsDir -ErrorAction SilentlyContinue
        $runs = @(Get-PhysicalTierRuns)
        $problems = for ($i = 0; $i -lt $runs.Count; $i++) {
            $run = $runs[$i]
            Write-Host "-- $($run.Name)"
            $resultsFile = Join-Path $resultsDir "run-$i.trx"
            $global:LASTEXITCODE = 0
            & $TestRunCommand $Configuration $run.Filter $resultsFile | Out-Host
            Get-RunProblems $run $resultsFile $LASTEXITCODE
        }
        if ($problems) {
            Write-Host 'Problems:'
            $problems | ForEach-Object { Write-Host "  $_" }
            $global:LASTEXITCODE = 1
        }
        else { $global:LASTEXITCODE = 0 }
    }
}
else {
    Invoke-Step "Tests, quarantined excluded ($Configuration)" { & $TestCommand $Configuration }
}

Invoke-Step "Script tests (Pester)" { & $ScriptTestCommand $Configuration }

if ($Quarantined) {
    Write-Host '== Quarantined tests (expected to fail, non-blocking)'
    dotnet test Test/Test.csproj --configuration $Configuration --no-build --nologo --filter 'Quarantine=true' |
        Select-String -Pattern 'Passed!|Failed!|Passed |Total tests|Passed:|Failed:'
    Write-Host '   Any quarantined test that passes should lose its [Quarantine] attribute.'
    $global:LASTEXITCODE = 0
}

Set-Content -Path $memory -Value $fingerprint -NoNewline
Write-Host "== Gate ($Configuration) passed."
exit 0
