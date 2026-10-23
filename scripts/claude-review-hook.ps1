<#
.SYNOPSIS
    Claude Code PostToolUse hook: after a shell command that creates a pull request or pushes, starts review-pr.ps1.

.DESCRIPTION
    Reads the hook payload from stdin. When the command ran `gh pr create` or a real `git push`, starts a review of
    the current branch's pull request in the background and returns at once, so the authoring session is not held up.
    Does nothing inside the reviewer session itself (MAYDAY_CLAUDE_REVIEWER), for other commands, or on malformed
    input. Always exits 0: a review is advisory and never blocks the session. See ADR 0010.

.PARAMETER StartReview
    Starts the review of the pull request of the branch checked out in the given directory (the payload's cwd, or
    $null when it has none). The default runs review-pr.ps1 in a hidden, detached PowerShell that logs to its own
    file in <git common dir>/claude-reviews. The script tests replace it.
#>
[CmdletBinding()]
param(
    [scriptblock] $StartReview = {
        param($Directory)
        $arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', "`"$PSScriptRoot\review-pr.ps1`"")
        if ($Directory) {
            $Directory = git -C $Directory rev-parse --show-toplevel
            $arguments += '-RepoRoot', "`"$Directory`""
        }
        else { $Directory = Split-Path -Parent $PSScriptRoot }
        $reviews = Join-Path (git -C $Directory rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        New-Item -ItemType Directory -Force -Path $reviews | Out-Null
        # One log per run: a review takes minutes, and a second one must not find the first one's log locked.
        $log = Join-Path $reviews "review-$(Get-Date -Format 'yyyyMMdd-HHmmss')-$PID.log"
        Start-Process powershell -WindowStyle Hidden -WorkingDirectory $Directory -ArgumentList $arguments `
            -RedirectStandardOutput $log -RedirectStandardError "$log.err"
        Write-Output "Started a separate Claude review of the pull request of $Directory; it posts to the PR. Log: $log"
    }
)

if ($env:MAYDAY_CLAUDE_REVIEWER) { exit 0 }

try { $payload = [Console]::In.ReadToEnd() | ConvertFrom-Json }
catch { exit 0 }
$command = $payload.tool_input.command
if (-not $command) { exit 0 }

$creates = $command -match '(^|[\s;&|(])gh\s+pr\s+create\b'
# git's own options may come before the subcommand: `git -C ../wt push`, `git -c key=value push`.
$pushes = $command -match '(^|[\s;&|(])git(\s+(-[cC]\s+\S+|--?[\w-]+(=\S+)?))*\s+push\b' -and $command -notmatch '--dry-run'
if ($creates -or $pushes) { & $StartReview $payload.cwd }
exit 0
