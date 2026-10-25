<#
.SYNOPSIS
    git pre-push hook: skips the gate for a documentation-only push, otherwise runs the full gate with the robot.

.DESCRIPTION
    Reads the refs being pushed from standard input, one line per ref as git writes them:
    "<local ref> <local sha> <remote ref> <remote sha>". A push is documentation-only when every file changed by
    every pushed ref is Markdown or lies under docs/, knowledge/ or Media/. Such a push needs no build or tests and
    passes at once. Any other push runs scripts/gate.ps1 in Release with -Physical, and its exit code decides the push.

    A ref is compared with the remote's commit when the remote already has one this repository knows, otherwise
    (a new branch) with its merge-base with -Upstream. Deleting a remote ref changes no files.

.PARAMETER RepoRoot
    The git working tree being pushed. Defaults to the repository this script lives in.

.PARAMETER Upstream
    The branch a new branch is compared with. Defaults to origin/master.

.PARAMETER GateScript
    The gate to run for a push that is not documentation-only. The script tests replace it with a fake.

.PARAMETER RefLines
    The pushed refs. Defaults to standard input, where git writes them; the script tests pass them directly.
#>
[CmdletBinding()]
param(
    [string] $RepoRoot,
    [string] $Upstream = 'origin/master',
    [string] $GateScript,
    [string[]] $RefLines
)

$ErrorActionPreference = 'Stop'
if (-not $RepoRoot) { $RepoRoot = Split-Path -Parent $PSScriptRoot }
if (-not $GateScript) { $GateScript = Join-Path $PSScriptRoot 'gate.ps1' }
if ($null -eq $RefLines) { $RefLines = [Console]::In.ReadToEnd() -split "`r?`n" }
Set-Location $RepoRoot

# As in gate.ps1: git sets these for hooks of a linked worktree, and they would override the repository found here.
'GIT_DIR', 'GIT_WORK_TREE', 'GIT_INDEX_FILE', 'GIT_COMMON_DIR', 'GIT_PREFIX', 'GIT_OBJECT_DIRECTORY',
    'GIT_ALTERNATE_OBJECT_DIRECTORIES' | ForEach-Object { Remove-Item -Path "Env:$_" -ErrorAction SilentlyContinue }

$noCommit = '0' * 40

function Test-DocumentationFile([string] $Path) {
    $Path -match '\.md$' -or $Path -match '^(docs|knowledge|Media)/'
}

function Get-ChangedFiles([string] $LocalSha, [string] $RemoteSha) {
    $ErrorActionPreference = 'Continue'
    git cat-file -e "$RemoteSha^{commit}" 2>$null
    $base = if ($RemoteSha -ne $noCommit -and $LASTEXITCODE -eq 0) { $RemoteSha } else { git merge-base $Upstream $LocalSha }
    if (-not $base) { throw "Cannot find where $LocalSha leaves $Upstream; fetch it first." }
    git diff --name-only $base $LocalSha
}

$changed = foreach ($line in $RefLines | Where-Object { $_.Trim() }) {
    $null, $localSha, $null, $remoteSha = $line.Trim() -split '\s+'
    if ($localSha -ne $noCommit) { Get-ChangedFiles $localSha $remoteSha }
}

$code = @($changed | Where-Object { -not (Test-DocumentationFile $_) })
if ($code.Count -eq 0) {
    Write-Host '== Documentation-only push; the gate is not needed.'
    exit 0
}

& powershell -NoProfile -ExecutionPolicy Bypass -File $GateScript -Configuration Release -Physical
exit $LASTEXITCODE
