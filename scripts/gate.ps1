<#
.SYNOPSIS
    Runs the local quality gate: build the solution, then run the blocking test set.

.DESCRIPTION
    The same checks as the optional GitHub workflow (.github/workflows/ci.yml): a build and the tests that are
    not marked [Quarantine]. Exits non-zero when any step fails.

    A pass is remembered per configuration against a fingerprint of the working tree (every non-Markdown file
    git tracks or could track). When the tree is unchanged since the last pass the gate returns immediately, so
    it can run on every Claude Code stop and every push without repeating work. Use -Force to ignore the memory.

    No robot is attached when the gate runs: MAYDAY_ROBOT_IS_CONNECTED is forced to False so a physical test can
    never try to move one. Main and EllieMain are never run.

.PARAMETER Configuration
    Build configuration. Release matches CI; Debug is faster for tight loops.

.PARAMETER Quarantined
    Also run the [Quarantine] tests, non-blocking, and report any that now pass.

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
    [Quarantine] tests.
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',
    [switch] $Quarantined,
    [switch] $Force,
    [string] $RepoRoot,
    [scriptblock] $BuildCommand = {
        param($Configuration)
        dotnet build Mayday.sln --configuration $Configuration --nologo --verbosity quiet
    },
    [scriptblock] $TestCommand = {
        param($Configuration)
        dotnet test Test/Test.csproj --configuration $Configuration --no-build --nologo --filter 'Quarantine!=true'
    }
)

$ErrorActionPreference = 'Stop'
if (-not $RepoRoot) { $RepoRoot = Split-Path -Parent $PSScriptRoot }
Set-Location $RepoRoot

$env:MAYDAY_ROBOT_IS_CONNECTED = 'False'

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

$gitDir = git rev-parse --absolute-git-dir
$memory = Join-Path $gitDir "gate-passed-$Configuration"
$fingerprint = Get-TreeFingerprint

if (-not $Force -and (Test-Path $memory) -and ((Get-Content $memory -Raw).Trim() -eq $fingerprint)) {
    Write-Host "== Gate ($Configuration) already passed for this exact working tree; skipping. Use -Force to rerun."
    exit 0
}

Invoke-Step "Build ($Configuration)" { & $BuildCommand $Configuration }

Invoke-Step "Tests, quarantined excluded ($Configuration)" { & $TestCommand $Configuration }

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
