<#
.SYNOPSIS
    Waits until the separate Claude review of the checked-out head commit is posted, then prints it.

.DESCRIPTION
    The authoring session runs this in the background after a push, so it is told when the review that
    claude-review-hook.ps1 started has arrived and can address its findings (ADR 0014). Looks up the pull request of
    the current branch and waits for review-pr.ps1's marker for that pull request and the working tree's HEAD in
    <git common dir>/claude-reviews. Exits 0 and prints the posted review once it is there, and exits 0 at once when
    there is no open, non-draft pull request to review. Exits 1 when the review of HEAD failed or none arrived within
    the timeout; run review-pr.ps1 then.

.PARAMETER RepoRoot
    The git working tree whose pull request and HEAD to wait for. Defaults to the repository this script lives in.

.PARAMETER TimeoutSeconds
    How long to wait for the review. It must outlast review-pr.ps1's wait for the pushed head (30 minutes by default)
    plus the review itself, which usually takes a few minutes.

.PARAMETER PollSeconds
    The pause between checks for the review.

.PARAMETER GetPullRequest
    Returns the current branch's pull request (number, isDraft, state) or $null for none, and throws when the lookup
    itself fails, which ends the wait with exit 1. The script tests replace it and -Sleep.

.PARAMETER Sleep
    Pauses for the given number of seconds between checks.
#>
[CmdletBinding()]
param(
    [string] $RepoRoot,
    [int] $TimeoutSeconds = 3600,
    [int] $PollSeconds = 15,
    [scriptblock] $GetPullRequest = {
        # Under Stop, Windows PowerShell turns gh's stderr ("no pull requests found") into an error, even with 2>$null.
        $ErrorActionPreference = 'Continue'
        $output = @(gh pr view --json number,isDraft,state 2>&1) | ForEach-Object { "$_" }
        # Only gh's own "no pull requests found" means there is none; offline or logged out must not look like it.
        if ($LASTEXITCODE -eq 0) { $output | Out-String | ConvertFrom-Json }
        elseif ($output -match 'no pull requests found') { $null }
        else { throw "gh pr view failed: $($output -join ' ')" }
    },
    [scriptblock] $Sleep = { param($Seconds) Start-Sleep -Seconds $Seconds }
)

$ErrorActionPreference = 'Stop'
if (-not $RepoRoot) { $RepoRoot = Split-Path -Parent $PSScriptRoot }
Set-Location $RepoRoot

$pr = & $GetPullRequest
if (-not $pr -or $pr.isDraft -or $pr.state -ne 'OPEN') {
    Write-Host '== No open, non-draft pull request for this branch; no review to wait for.'
    exit 0
}

# The head just pushed is HEAD here; the pull request's own head can lag behind GitHub for a moment.
$sha = (git rev-parse HEAD).Trim()
$reviews = Join-Path (git rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
$posted = Join-Path $reviews "$($pr.number)-$sha"
$failed = "$posted.failed"
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
Write-Host "== Waiting for the review of #$($pr.number) at $sha"
while ($true) {
    if (Test-Path $posted) {
        Write-Host "== The review of #$($pr.number) at $sha was posted:"
        Write-Output (Get-Content "$posted.md" -Raw)
        exit 0
    }
    if (Test-Path $failed) {
        Write-Host "== FAILED: the review of #$($pr.number) at $sha failed; see the logs in $reviews and run review-pr.ps1 -PullRequest $($pr.number)."
        exit 1
    }
    if ((Get-Date) -ge $deadline) {
        Write-Host "== FAILED: no review of #$($pr.number) at $sha within $TimeoutSeconds s; see the logs in $reviews and run review-pr.ps1 -PullRequest $($pr.number)."
        exit 1
    }
    & $Sleep $PollSeconds
}
