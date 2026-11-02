<#
.SYNOPSIS
    Has a separate, headless Claude Code session review a pull request and posts the review as a PR comment.

.DESCRIPTION
    Looks up the pull request (the current branch's when -PullRequest is omitted), checks its head commit out into a
    temporary worktree, runs a fresh `claude -p` session there with no history of the authoring session, and posts its
    output with `gh pr comment`. The reviewer only reads and reports: it cannot edit, push, approve or merge. Draft and
    closed pull requests are skipped, and a head commit that was already reviewed is skipped unless -Force. Exits 0
    when it reviewed or skipped, non-zero when the review produced nothing or could not be posted; a failure is marked
    in <git common dir>/claude-reviews, so wait-for-review.ps1 stops waiting. See ADR 0010 and ADR 0014.

.PARAMETER PullRequest
    The pull request number. Defaults to the pull request of the current branch.

.PARAMETER Force
    Review even when this head commit was already reviewed.

.PARAMETER RepoRoot
    The git working tree whose pull request to review. Defaults to the repository this script lives in.

.PARAMETER Head
    The commit to review. When given, waits up to -HeadTimeoutSeconds for the pull request's head to become this
    commit, as it does when the push that started the review is still running, and fails, marking it failed, when it
    does not.

.PARAMETER HeadTimeoutSeconds
    How long to wait for the pull request's head to become -Head. The pre-push gate can take many minutes.

.PARAMETER PollSeconds
    The pause between checks of the pull request's head.

.PARAMETER Sleep
    Pauses for the given number of seconds between checks.

.PARAMETER GetPullRequest
    Returns the pull request (number, headRefOid, baseRefName, isDraft, state) or $null for none, and throws when the
    lookup itself fails. Takes the number, or $null for the current branch. The script tests replace it, -Review, -PostComment and -Sleep.

.PARAMETER Review
    Runs the reviewer on a prompt in a directory and returns its review as text, setting $LASTEXITCODE.

.PARAMETER PostComment
    Posts the file at the given path as a comment on the given pull request, setting $LASTEXITCODE.
#>
[CmdletBinding()]
param(
    [int] $PullRequest,
    [switch] $Force,
    [string] $RepoRoot,
    [string] $Head,
    [int] $HeadTimeoutSeconds = 1800,
    [int] $PollSeconds = 15,
    [scriptblock] $Sleep = { param($Seconds) Start-Sleep -Seconds $Seconds },
    [scriptblock] $GetPullRequest = {
        param($Number)
        # Under Stop, Windows PowerShell turns gh's stderr ("no pull requests found") into an error, even with 2>$null.
        $ErrorActionPreference = 'Continue'
        $fields = 'number,headRefOid,baseRefName,isDraft,state'
        $output = @(if ($Number) { gh pr view $Number --json $fields 2>&1 } else { gh pr view --json $fields 2>&1 }) |
            ForEach-Object { "$_" }
        # Only gh's own "no pull requests found" means there is none; offline or logged out must not look like it.
        if ($LASTEXITCODE -eq 0) { $output | Out-String | ConvertFrom-Json }
        elseif ($output -match 'no pull requests found') { $null }
        else { throw "gh pr view failed: $($output -join ' ')" }
    },
    [scriptblock] $Review = {
        param($Prompt, $Directory)
        # Windows PowerShell pipes ASCII to native commands by default; the prompt and review need UTF-8.
        $OutputEncoding = [Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
        Push-Location $Directory
        try {
            # User settings only: the project's hooks (the Stop gate, this review hook) must not run in the reviewer.
            # Linear is read-only: the user's claude.ai Linear connector, to look up issue and project titles for links.
            $Prompt | claude -p --setting-sources user --no-session-persistence --output-format text `
                --allowedTools ('Read,Grep,Glob,Bash(gh pr diff:*),Bash(gh pr view:*),Bash(git diff:*),Bash(git log:*),Bash(git show:*),' +
                    'mcp__claude_ai_Linear__get_issue,mcp__claude_ai_Linear__get_project,mcp__claude_ai_Linear__list_issues,mcp__claude_ai_Linear__list_projects') |
                Out-String
        }
        finally { Pop-Location }
    },
    [scriptblock] $PostComment = {
        param($Number, $BodyFile)
        gh pr comment $Number --body-file $BodyFile
    }
)

$ErrorActionPreference = 'Stop'
if (-not $RepoRoot) { $RepoRoot = Split-Path -Parent $PSScriptRoot }
Set-Location $RepoRoot

$number = if ($PullRequest) { $PullRequest } else { $null }
$pr = & $GetPullRequest $number
if (-not $pr) {
    Write-Host '== No pull request for this branch; nothing to review.'
    exit 0
}
if ($pr.isDraft -or $pr.state -ne 'OPEN') {
    Write-Host "== Pull request #$($pr.number) is a draft or not open; not reviewing."
    exit 0
}

$reviews = Join-Path (git rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
New-Item -ItemType Directory -Force -Path $reviews | Out-Null
$sha = if ($Head) { $Head } else { $pr.headRefOid }
$memory = Join-Path $reviews "$($pr.number)-$sha"
if (-not $Force -and (Test-Path $memory)) {
    Write-Host "== #$($pr.number) at $sha was already reviewed; skipping. Use -Force to review again."
    exit 0
}
# One run per head. A push of the same commit again, after a failed push, starts a second run while the first may still
# wait for that head with little of its time left, so the newer run takes over a lock that is still waiting and the
# older one stands down; a lock that is reviewing makes the newer run skip. A lock older than the longest run is left
# by a killed run and taken over.
$lock = "$memory.lock"
$token = [guid]::NewGuid().ToString('N')
if ((Test-Path $lock) -and (Get-Item $lock).LastWriteTime -gt (Get-Date).AddSeconds(-($HeadTimeoutSeconds + 1800)) -and
    (Get-Content $lock -Raw) -notmatch '^waiting ') {
    Write-Host "== Another run is reviewing #$($pr.number) at $sha; skipping."
    exit 0
}
Set-Content -Path $lock -Value "waiting $token"
# wait-for-review.ps1 stops waiting on a head marked failed, so a new run clears an earlier failure's mark first.
$failed = "$memory.failed"
Remove-Item -Path $failed -ErrorAction SilentlyContinue

function Test-LockIsOurs { (Test-Path $lock) -and (Get-Content $lock -Raw) -match " $token\s*$" }
# Every exit from here on releases the lock while it is ours, and a failure marks the head failed.
function Exit-Review([int] $Code, [switch] $MarkFailed) {
    if ($MarkFailed) { Set-Content -Path $failed -Value (Get-Date -Format o) }
    if (Test-LockIsOurs) { Remove-Item -Path $lock -ErrorAction SilentlyContinue }
    exit $Code
}
# A trap covers its whole scope, so it also sees errors raised before $lock is known; those mark nothing.
trap {
    Write-Host "== FAILED: $_"
    if ($lock) { Exit-Review 1 -MarkFailed }
    exit 1
}

$deadline = (Get-Date).AddSeconds($HeadTimeoutSeconds)
while ($pr.headRefOid -ne $sha -and (Get-Date) -lt $deadline) {
    & $Sleep $PollSeconds
    if (-not (Test-LockIsOurs)) {
        Write-Host "== A newer run took over #$($pr.number) at $sha; standing down."
        exit 0
    }
    # A lookup that fails for a moment keeps the last pull request seen.
    try { $next = & $GetPullRequest $number } catch { $next = $null }
    if ($next) { $pr = $next }
}
if ($pr.headRefOid -ne $sha) {
    Write-Host "== FAILED: the head of #$($pr.number) did not become $sha within $HeadTimeoutSeconds s"
    Exit-Review 1 -MarkFailed
}
Set-Content -Path $lock -Value "reviewing $token"

$ErrorActionPreference = 'Continue'
git cat-file -e "$sha^{commit}" 2>$null
if ($LASTEXITCODE -ne 0) { git fetch -q origin $sha }
$checkout = Join-Path ([IO.Path]::GetTempPath()) "mayday-review-$($pr.number)-$([guid]::NewGuid().ToString('N'))"
git worktree add -q --detach $checkout $sha
if ($LASTEXITCODE -ne 0) {
    Write-Host "== FAILED: could not check out $sha"
    Exit-Review 1 -MarkFailed
}

$prompt = @"
You are an independent reviewer of pull request #$($pr.number) into $($pr.baseRefName), head commit $sha. You did not
write it. The working directory is a checkout of that commit; `gh pr diff $($pr.number)` shows the change.

Before reviewing, read CLAUDE.md, the CLAUDE.md and README.md of every project the diff touches, the ADRs in docs/adr
that cover the affected boundaries, and knowledge/INDEX.md. Review against those rules.

Look for, in order of importance:
- Correctness bugs and missed edge cases.
- Physical safety: weakened joint limits, velocity limits or other motion constraints; code or tests that could start
  or move real hardware (Main, EllieMain, PhysicalRobotFact/PhysicalRobotTheory).
- Physical quantities as bare numbers instead of UnitsNet types.
- Architecture: dependencies pointing outward, robot-specific or hardware dependencies in RobotDomain or Generic,
  reaching into another namespace's Internal types, boundary changes without an ADR.
- Tests: behavior changes without a test that fails against the old behavior; changed expectations in existing tests;
  refactorings that touch tests; untested scripts or CI configuration.
- Documentation: XML docs that leak implementation into a contract; README or diagrams left stale.
- Commits: one intent per commit, subject format, Problem/Rationale/Impact bodies. Unfolded fixup! and amend! commits
  are how review fixes are meant to arrive (CLAUDE.md, Review fixes): check what they change like any other diff, but
  do not report their subject or body format; at most note once that they must be folded before merge.

Give every Linear issue or project you mention as a link with its ID and title (CLAUDE.md, Linear issues); look it up
with the read-only Linear tools rather than from memory, and if a lookup fails, give the ID with its URL.

Report only issues you are confident about, each with file:line and the concrete failure it causes; say nothing about
style the repository does not ask for. Your final message is posted verbatim as the review comment, so make it only
the review in Markdown: a one-line verdict (exactly ``No findings.`` when you have none: the author fixes and pushes
until a review says so), the findings ranked most severe first, then what you did not check. Do not
post it yourself, approve, request changes or merge: the owner decides.
"@

$env:MAYDAY_CLAUDE_REVIEWER = '1'
Write-Host "== Reviewing #$($pr.number) at $sha"
try {
    $global:LASTEXITCODE = 0
    $text = & $Review $prompt $checkout
    $reviewExit = $LASTEXITCODE
}
finally {
    git -C $RepoRoot worktree remove --force $checkout
    Remove-Item Env:MAYDAY_CLAUDE_REVIEWER
}

$text = ($text | Out-String).Trim()
if ($reviewExit -ne 0 -or -not $text) {
    Write-Host "== FAILED: the reviewer produced no review (exit $reviewExit)"
    Exit-Review 1 -MarkFailed
}

$body = Join-Path $reviews "$($pr.number)-$sha.md"
# Windows PowerShell's UTF8 encoding writes a BOM, which would show at the start of the comment.
[IO.File]::WriteAllText($body, @"
## Claude review of $sha

_A separate Claude Code session reviewed this commit ([ADR 0010](docs/adr/0010-local-claude-review-of-every-pull-request.md)). Advisory: the owner decides._

$text
"@, [Text.UTF8Encoding]::new($false))
$global:LASTEXITCODE = 0
& $PostComment $pr.number $body
if ($LASTEXITCODE -ne 0) {
    $postExit = $LASTEXITCODE
    Write-Host "== FAILED: could not post the review (exit $postExit); it is in $body"
    Exit-Review $postExit -MarkFailed
}
Set-Content -Path $memory -Value (Get-Date -Format o)
Write-Host "== Posted the review of #$($pr.number)."
Exit-Review 0
