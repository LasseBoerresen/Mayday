<#
.SYNOPSIS
    Claude Code PostToolUse hook: after a shell command that creates a pull request or pushes, starts review-pr.ps1.

.DESCRIPTION
    Reads the hook payload from stdin. When the command ran `gh pr create` or a real `git push`, starts a review of
    the current branch's pull request in the background and returns at once, so the authoring session is not held up.
    It then tells the session, as additional context, to wait for the review with wait-for-review.ps1 and address its
    findings until a review of the head has none (ADR 0014). Does nothing inside the reviewer session itself
    (MAYDAY_CLAUDE_REVIEWER), for other commands, or on malformed input. Always exits 0: a review is advisory and
    never blocks the session. See ADR 0010.

.PARAMETER StartReview
    Starts the review of the pull request of the branch checked out in the given directory (the payload's cwd, or
    $null when it has none), at the given head commit (that directory's HEAD, or $null when unknown). The default
    runs review-pr.ps1 in a hidden, detached PowerShell that logs to its own file in <git common dir>/claude-reviews.
    The script tests replace it.
#>
[CmdletBinding()]
param(
    [scriptblock] $StartReview = {
        param($Directory, $Head)
        $arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', "`"$PSScriptRoot\review-pr.ps1`"")
        if ($Head) { $arguments += '-Head', $Head }
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
$argument = '(''[^'']*''|"[^"]*"|\S+)'
$gitOptions = "((\s+(-[cC]\s+$argument|--?[\w-]+(=\S+)?))*)"
$push = [regex]::Match($command, "(^|[\s;&|(])git$gitOptions\s+push\b")
$pushes = $push.Success -and $command -notmatch '--dry-run'
if (-not ($creates -or $pushes)) { exit 0 }

# The pushed repository is the one `git -C <dir>` names, if any, not the session's working directory.
$directory = $payload.cwd
$dashC = [regex]::Match($push.Groups[2].Value, "-C\s+$argument")
if ($dashC.Success) {
    $directory = $dashC.Groups[1].Value.Trim('''', '"')
    if ($payload.cwd -and -not [IO.Path]::IsPathRooted($directory)) { $directory = Join-Path $payload.cwd $directory }
    $directory = [IO.Path]::GetFullPath($directory)
}

# A command that also moves HEAD (`git commit ... && git push`) may still be doing so; HEAD read now could be the
# commit before the pushed one, so neither a review nor a wait would follow the pushed commit. The session pushes
# again in its own command instead, which reaches the code below.
# Quoted text, such as a pull request body that mentions `git commit`, runs nothing.
$unquoted = $command -replace '"[^"]*"|''[^'']*''', '""'
$movesHead = $unquoted -match "(^|[\s;&|(])git$gitOptions\s+(commit|rebase|merge|cherry-pick|reset|am|pull)\b"
if ($movesHead) {
    @{
        hookSpecificOutput = @{
            hookEventName     = 'PostToolUse'
            additionalContext = 'This command also moved HEAD, so the review hook could not tell which commit it pushes ' +
                'and started no review (ADR 0014). Once it has finished, push again in its own command; that push ' +
                'starts the review and says how to wait for it.'
        }
    } | ConvertTo-Json -Compress
    exit 0
}

# The hook runs as soon as a background push starts, before the pull request has the pushed head; review-pr.ps1
# waits for this one.
$head = $null
if ($directory) {
    $ErrorActionPreference = 'Continue'
    $head = git -C $directory rev-parse HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { $head = $null }
}
if ($head) {
    # The session's wait trusts any failure mark of this head, so one left by an earlier push of the same commit goes
    # now, before the session can start waiting; the detached run might clear it only after the wait has read it.
    # The same goes for an earlier run's mark that no pull request came for this head.
    $reviews = Join-Path (git -C $directory rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
    Remove-Item -Path (Join-Path $reviews "*-$head.failed"), (Join-Path $reviews "none-$head") -ErrorAction SilentlyContinue
}
$started = & $StartReview $directory $head | Out-String
$wait = "powershell -NoProfile -ExecutionPolicy Bypass -File `"$PSScriptRoot\wait-for-review.ps1`""
if ($directory) { $wait += " -RepoRoot `"$directory`"" }
if ($head) { $wait += " -Head $head" }
# Claude Code shows a PostToolUse hook's plain stdout only in the transcript; additionalContext reaches the session.
@{
    hookSpecificOutput = @{
        hookEventName     = 'PostToolUse'
        additionalContext = @"
$($started.Trim())
Follow this review before asking the owner to review or merge (ADR 0014): run ``$wait`` in the background now, with a
timeout of 3900000 ms if the tool takes one; it ends and prints the review once it is posted. Address every finding: fix it with a fixup commit, or reply on the pull request
why not. Push in its own command, not chained after a commit, so this hook names the pushed commit; that starts a
review of the new head. Wait again. Repeat until a review of the head says
``No findings.``, then fold the fixups as CLAUDE.md says.
"@
    }
} | ConvertTo-Json -Compress
exit 0
