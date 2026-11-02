#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
# Tests for scripts/review-pr.ps1, scripts/claude-review-hook.ps1 (ADR 0010) and scripts/wait-for-review.ps1 (ADR 0014).
#
# Each review test gets a scratch git repository with one commit and runs the script in a child PowerShell, because
# the scripts call `exit`. gh and claude are replaced with commands that append to a log, so nothing reaches GitHub
# or a model. Names follow the repo's Given/When/Then convention.

BeforeAll {
    $script:ReviewPr = Join-Path $PSScriptRoot '..\review-pr.ps1' | Resolve-Path | Select-Object -ExpandProperty Path
    $script:Hook = Join-Path $PSScriptRoot '..\claude-review-hook.ps1' | Resolve-Path | Select-Object -ExpandProperty Path
    $script:RepoRoot = Join-Path $PSScriptRoot '..\..' | Resolve-Path | Select-Object -ExpandProperty Path

    # Runs powershell on a wrapper script and returns its exit code and combined output.
    function Invoke-Wrapper([string] $Body, [string] $InputText = '') {
        $wrapper = Join-Path $TestDrive "wrapper-$([guid]::NewGuid().ToString('N')).ps1"
        Set-Content -Path $wrapper -Value $Body
        $ErrorActionPreference = 'Continue'
        $output = $InputText | & powershell -NoProfile -ExecutionPolicy Bypass -File $wrapper 2>&1 | ForEach-Object { "$_" } | Out-String
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output }
    }

    # Runs review-pr.ps1 against $script:Repo. The script block bodies stand in for gh and claude; LOG is replaced by
    # the log path and SHA by the scratch repository's head commit.
    function Invoke-ReviewPr(
        [string] $PullRequest = '[pscustomobject]@{ number = 7; headRefOid = ''SHA''; baseRefName = ''master''; isDraft = $false; state = ''OPEN'' }',
        [string] $Review = 'param($Prompt, $Directory) Add-Content -Path ''LOG'' -Value "review $(git -C $Directory rev-parse HEAD) reviewer=$env:MAYDAY_CLAUDE_REVIEWER"; Add-Content -Path ''LOG'' -Value "dir $Directory"; Set-Content -Path ''LOG.prompt'' -Value $Prompt; ''Looks fine.''',
        [switch] $Force,
        [string] $Head,
        [int] $HeadTimeoutSeconds = 60,
        [string] $PostComment = 'param($Number, $BodyFile) Add-Content -Path ''LOG'' -Value "post $Number $(Get-Content $BodyFile -Raw)"') {
        $forceArg = if ($Force) { '-Force' } else { '' }
        $headArg = if ($Head) { "-Head '$Head' -HeadTimeoutSeconds $HeadTimeoutSeconds -PollSeconds 1 -Sleep { param(`$Seconds) }" } else { '' }
        $body = @"
& '$script:ReviewPr' -RepoRoot '$script:Repo' $forceArg $headArg ``
    -GetPullRequest { $PullRequest } ``
    -Review { $Review } ``
    -PostComment { $PostComment }
exit `$LASTEXITCODE
"@.Replace('LOG', $script:Log).Replace('SHA', $script:Sha)
        Invoke-Wrapper $body
    }

    function Get-Log { if (Test-Path $script:Log) { @(Get-Content $script:Log) } else { @() } }

    # A directory with a gh.cmd that, like gh on a branch without a pull request, writes to stderr and exits 1. A real
    # native command, because Windows PowerShell turns native stderr into an error under Stop, even with 2>$null.
    function New-GhWithoutPullRequest([string] $Message = 'no pull requests found for branch "feature"') {
        $bin = Join-Path $TestDrive "bin-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Path $bin | Out-Null
        Set-Content -Path (Join-Path $bin 'gh.cmd') -Value "@echo $Message 1>&2`r`n@exit /b 1"
        $bin
    }
}

Describe 'review-pr.ps1' {
    BeforeEach {
        $script:Repo = Join-Path $TestDrive "repo-$([guid]::NewGuid().ToString('N'))"
        $script:Log = Join-Path $TestDrive "log-$([guid]::NewGuid().ToString('N')).txt"
        New-Item -ItemType Directory -Path $script:Repo | Out-Null
        git -C $script:Repo init -q
        Set-Content -Path (Join-Path $script:Repo 'Code.cs') -Value 'class A {}'
        git -C $script:Repo add . 2>$null
        git -C $script:Repo -c user.name=t -c user.email=t@t commit -q -m init
        $script:Sha = (git -C $script:Repo rev-parse HEAD).Trim()
    }

    It 'GivenOpenPullRequest_WhenRun_ThenReviewsItsHeadAndPostsTheReview' {
        # When
        $result = Invoke-ReviewPr

        # Then
        $result.ExitCode | Should -Be 0
        $log = Get-Log
        $log | Should -Contain "review $script:Sha reviewer=1"
        ($log -join "`n") | Should -Match "post 7 [\s\S]*$script:Sha[\s\S]*Looks fine\."
    }

    It 'GivenOpenPullRequest_WhenRun_ThenReviewsInASeparateCheckoutRemovedAfterwards' {
        # When
        Invoke-ReviewPr | Out-Null

        # Then
        $directory = (Get-Log | Where-Object { $_ -like 'dir *' }) -replace '^dir ', ''
        $directory | Should -Not -BeNullOrEmpty
        $directory | Should -Not -Be $script:Repo
        $directory | Should -Not -Exist
        (git -C $script:Repo worktree list | Measure-Object).Count | Should -Be 1
    }

    It 'GivenOpenPullRequest_WhenRun_ThenPromptNamesThePullRequestAndTheRepositoryRules' {
        # When
        Invoke-ReviewPr | Out-Null

        # Then
        $prompt = Get-Content "$script:Log.prompt" -Raw
        $prompt | Should -Match '#7'
        $prompt | Should -Match 'CLAUDE\.md'
        $prompt | Should -Match 'docs/adr'
    }

    It 'GivenOpenPullRequest_WhenRun_ThenPromptTreatsUnfoldedFixupsAsFoldBeforeMergeNotFormatViolations' {
        # Given: CLAUDE.md asks for review fixes as fixup!/amend! commits, folded only once the review is satisfied

        # When
        Invoke-ReviewPr | Out-Null

        # Then
        $prompt = Get-Content "$script:Log.prompt" -Raw
        $prompt | Should -Match 'fixup!'
        $prompt | Should -Match 'amend!'
        $prompt | Should -Match 'fold'
    }

    It 'GivenOpenPullRequest_WhenRun_ThenPromptAsksForLinearReferencesAsTitledLinksLookedUpInLinear' {
        # When
        Invoke-ReviewPr | Out-Null

        # Then
        $prompt = Get-Content "$script:Log.prompt" -Raw
        $prompt | Should -Match 'Linear issue or project'
        $prompt | Should -Match 'ID and title'
        $prompt | Should -Match 'look (it|them) up'
    }

    It 'GivenHeadAlreadyReviewed_WhenRunAgain_ThenSkips' {
        # Given
        Invoke-ReviewPr | Out-Null

        # When
        $result = Invoke-ReviewPr

        # Then
        $result.ExitCode | Should -Be 0
        @(Get-Log | Where-Object { $_ -like 'review *' }).Count | Should -Be 1
    }

    It 'GivenHeadAlreadyReviewed_WhenForced_ThenReviewsAgain' {
        # Given
        Invoke-ReviewPr | Out-Null

        # When
        Invoke-ReviewPr -Force | Out-Null

        # Then
        @(Get-Log | Where-Object { $_ -like 'review *' }).Count | Should -Be 2
    }

    It 'GivenDraftPullRequest_WhenRun_ThenSkips' {
        # When
        $result = Invoke-ReviewPr -PullRequest "[pscustomobject]@{ number = 7; headRefOid = 'SHA'; baseRefName = 'master'; isDraft = `$true; state = 'OPEN' }"

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -BeNullOrEmpty
    }

    It 'GivenNoPullRequest_WhenRun_ThenSkips' {
        # When
        $result = Invoke-ReviewPr -PullRequest '$null'

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -BeNullOrEmpty
    }

    It 'GivenReviewProducesNothing_WhenRun_ThenFailsWithoutPostingAndReviewsAgainNextTime' {
        # Given
        $failed = Invoke-ReviewPr -Review 'param($Prompt, $Directory) Add-Content -Path ''LOG'' -Value review; $global:LASTEXITCODE = 1'

        # When
        $result = Invoke-ReviewPr

        # Then
        $failed.ExitCode | Should -Be 1
        $result.ExitCode | Should -Be 0
        $log = Get-Log
        @($log | Where-Object { $_ -like 'review*' }).Count | Should -Be 2
        @($log | Where-Object { $_ -like 'post *' }).Count | Should -Be 1
    }

    It 'GivenReviewProducesNothing_WhenRun_ThenMarksTheHeadFailedUntilAReviewIsPosted' {
        # Given: wait-for-review.ps1 stops waiting on a head whose review failed
        $reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        $failedMarker = Join-Path $reviews "7-$script:Sha.failed"

        # When
        Invoke-ReviewPr -Review 'param($Prompt, $Directory) $global:LASTEXITCODE = 1' | Out-Null
        $markedAfterFailure = Test-Path $failedMarker
        Invoke-ReviewPr | Out-Null

        # Then
        $markedAfterFailure | Should -BeTrue
        $failedMarker | Should -Not -Exist
    }

    It 'GivenEarlierFailureMarked_WhenReviewingAgain_ThenClearsTheMarkBeforeTheReviewerRuns' {
        # Given: a wait started during this review must not see the earlier failure and stop
        $reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        New-Item -ItemType Directory -Force -Path $reviews | Out-Null
        $failedMarker = Join-Path $reviews "7-$script:Sha.failed"
        Set-Content -Path $failedMarker -Value 'earlier'

        # When
        Invoke-ReviewPr -Review "param(`$Prompt, `$Directory) Add-Content -Path 'LOG' -Value ""marked `$(Test-Path '$failedMarker')""; 'Looks fine.'" | Out-Null

        # Then
        Get-Log | Should -Contain 'marked False'
    }

    It 'GivenReview<Case>_WhenRun_ThenFailsAndMarksTheHeadFailed' -ForEach @(
        @{ Case = 'CannotBePosted'; Review = ''; PostComment = 'param($Number, $BodyFile) $global:LASTEXITCODE = 1' },
        @{ Case = 'Throws'; Review = 'param($Prompt, $Directory) throw ''reviewer crashed'''; PostComment = '' }
    ) {
        # Given
        $arguments = @{}
        if ($Review) { $arguments.Review = $Review }
        if ($PostComment) { $arguments.PostComment = $PostComment }

        # When
        $result = Invoke-ReviewPr @arguments

        # Then
        $result.ExitCode | Should -Not -Be 0
        $reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        Join-Path $reviews "7-$script:Sha.failed" | Should -Exist
        Join-Path $reviews "7-$script:Sha" | Should -Not -Exist
    }

    It 'GivenHeadCannotBeCheckedOut_WhenRun_ThenFailsAndMarksTheHeadFailed' {
        # Given: a head commit that is neither in the repository nor fetchable
        $missing = '0' * 40

        # When
        $result = Invoke-ReviewPr -PullRequest "[pscustomobject]@{ number = 7; headRefOid = '$missing'; baseRefName = 'master'; isDraft = `$false; state = 'OPEN' }"

        # Then
        $result.ExitCode | Should -Be 1
        $reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        Join-Path $reviews "7-$missing.failed" | Should -Exist
    }

    It 'GivenPushedHeadNotYetOnThePullRequest_WhenRunWithHead_ThenWaitsForItAndReviewsIt' {
        # Given: a push still running in the background (the pre-push gate takes minutes) when the hook starts the
        # review, so GitHub still shows the previous head for a while
        $calls = Join-Path $TestDrive "calls-$([guid]::NewGuid().ToString('N')).txt"
        $pullRequest = "Add-Content -Path '$calls' -Value x; `$head = if (@(Get-Content '$calls').Count -lt 3) { 'old' } else { 'SHA' }; [pscustomobject]@{ number = 7; headRefOid = `$head; baseRefName = 'master'; isDraft = `$false; state = 'OPEN' }"

        # When
        $result = Invoke-ReviewPr -PullRequest $pullRequest -Head $script:Sha

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -Contain "review $script:Sha reviewer=1"
    }

    It 'GivenEarlierFailureMarked_WhenRunWithHead_ThenClearsTheMarkBeforeWaitingForTheHead' {
        # Given: a push that failed earlier left its head marked failed, and the same commit is pushed again
        $reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        New-Item -ItemType Directory -Force -Path $reviews | Out-Null
        $failedMarker = Join-Path $reviews "7-$script:Sha.failed"
        Set-Content -Path $failedMarker -Value 'earlier'
        $calls = Join-Path $TestDrive "calls-$([guid]::NewGuid().ToString('N')).txt"
        $pullRequest = "Add-Content -Path '$calls' -Value x; Add-Content -Path '$script:Log' -Value ""marked `$(Test-Path '$failedMarker')""; `$head = if (@(Get-Content '$calls').Count -lt 3) { 'old' } else { 'SHA' }; [pscustomobject]@{ number = 7; headRefOid = `$head; baseRefName = 'master'; isDraft = `$false; state = 'OPEN' }"

        # When
        Invoke-ReviewPr -PullRequest $pullRequest -Head $script:Sha | Out-Null

        # Then: the first lookup finds the pull request; every check of its head after that sees no mark
        $marks = @(Get-Log | Where-Object { $_ -like 'marked *' })
        $marks[1..($marks.Count - 1)] | Should -Not -Contain 'marked True'
    }

    It 'GivenAnotherRunIsReviewingTheHead_WhenRun_ThenSkipsWithoutReviewing' {
        # Given
        $reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        New-Item -ItemType Directory -Force -Path $reviews | Out-Null
        Set-Content -Path (Join-Path $reviews "7-$script:Sha.lock") -Value 'reviewing other-run'

        # When
        $result = Invoke-ReviewPr -Head $script:Sha

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -BeNullOrEmpty
    }

    It 'GivenAnotherRunIsStillWaitingForTheHead_WhenRun_ThenTakesOverAndReviewsIt' {
        # Given: the run of an earlier, failed push of the same commit still waits for it, and may run out of time
        # before this push lands
        $reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        New-Item -ItemType Directory -Force -Path $reviews | Out-Null
        $lock = Join-Path $reviews "7-$script:Sha.lock"
        Set-Content -Path $lock -Value 'waiting other-run'

        # When
        $result = Invoke-ReviewPr -Head $script:Sha

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -Contain "review $script:Sha reviewer=1"
        $lock | Should -Not -Exist
    }

    It 'GivenItsLockIsTakenOverWhileWaitingForTheHead_WhenRunWithHead_ThenStandsDownWithoutMarkingAFailure' {
        # Given: a newer push of the same commit takes the lock while this run still waits for the head
        $reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        $lock = Join-Path $reviews "7-$script:Sha.lock"
        $pullRequest = "[pscustomobject]@{ number = 7; headRefOid = 'old'; baseRefName = 'master'; isDraft = `$false; state = 'OPEN' }"
        $body = @"
& '$script:ReviewPr' -RepoRoot '$script:Repo' -Head '$script:Sha' -HeadTimeoutSeconds 60 -PollSeconds 1 ``
    -Sleep { param(`$Seconds) Set-Content -Path '$lock' -Value 'waiting newer-run' } ``
    -GetPullRequest { $pullRequest } ``
    -Review { param(`$Prompt, `$Directory) Add-Content -Path '$script:Log' -Value review; 'Looks fine.' }
exit `$LASTEXITCODE
"@

        # When
        $result = Invoke-Wrapper $body

        # Then
        $result.ExitCode | Should -Be 0
        Join-Path $reviews "7-$script:Sha.failed" | Should -Not -Exist
        Get-Content $lock | Should -Be 'waiting newer-run'
        Get-Log | Should -BeNullOrEmpty
    }

    It 'GivenLookupFailsForAMomentWhileWaitingForTheHead_WhenRunWithHead_ThenKeepsWaitingAndReviews' {
        # Given
        $calls = Join-Path $TestDrive "calls-$([guid]::NewGuid().ToString('N')).txt"
        $pullRequest = "Add-Content -Path '$calls' -Value x; `$n = @(Get-Content '$calls').Count; if (`$n -eq 2) { throw 'gh pr view failed: offline' }; `$head = if (`$n -lt 3) { 'old' } else { 'SHA' }; [pscustomobject]@{ number = 7; headRefOid = `$head; baseRefName = 'master'; isDraft = `$false; state = 'OPEN' }"

        # When
        $result = Invoke-ReviewPr -PullRequest $pullRequest -Head $script:Sha

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -Contain "review $script:Sha reviewer=1"
    }

    It 'GivenStaleLockOnTheHead_WhenRun_ThenReviewsIt' {
        # Given: a run that was killed (the machine slept) left its lock behind
        $reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        New-Item -ItemType Directory -Force -Path $reviews | Out-Null
        $lock = Join-Path $reviews "7-$script:Sha.lock"
        Set-Content -Path $lock -Value 'killed run'
        (Get-Item $lock).LastWriteTime = (Get-Date).AddHours(-2)

        # When
        Invoke-ReviewPr | Out-Null

        # Then
        Get-Log | Should -Contain "review $script:Sha reviewer=1"
        $lock | Should -Not -Exist
    }

    It 'GivenNoPullRequestOnGitHub_WhenRunWithTheDefaultLookup_ThenSkipsAndExitsZero' {
        # Given: a push of a new branch, before gh pr create
        $bin = New-GhWithoutPullRequest

        # When
        $result = Invoke-Wrapper @"
`$env:PATH = '$bin;' + `$env:PATH
& '$script:ReviewPr' -RepoRoot '$script:Repo'
exit `$LASTEXITCODE
"@

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'No pull request'
    }

    It 'GivenGhFailsForAnotherReason_WhenRunWithTheDefaultLookup_ThenFails' {
        # Given: gh offline or logged out, which is not the same as a branch without a pull request
        $bin = New-GhWithoutPullRequest 'error connecting to api.github.com'

        # When
        $result = Invoke-Wrapper @"
`$env:PATH = '$bin;' + `$env:PATH
& '$script:ReviewPr' -RepoRoot '$script:Repo'
exit `$LASTEXITCODE
"@

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'error connecting'
    }

    It 'GivenPullRequestCreatedAfterThePush_WhenRunWithHead_ThenWaitsForItAndReviewsTheHead' {
        # Given: `git push && gh pr create` in one background command fires the hook before the pull request exists
        $calls = Join-Path $TestDrive "calls-$([guid]::NewGuid().ToString('N')).txt"
        $pullRequest = "Add-Content -Path '$calls' -Value x; if (@(Get-Content '$calls').Count -lt 3) { `$null } else { [pscustomobject]@{ number = 7; headRefOid = 'SHA'; baseRefName = 'master'; isDraft = `$false; state = 'OPEN' } }"

        # When
        $result = Invoke-ReviewPr -PullRequest $pullRequest -Head $script:Sha

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -Contain "review $script:Sha reviewer=1"
    }

    It 'GivenNoPullRequestEverCreated_WhenRunWithHead_ThenExitsZeroWithoutReviewing' {
        # When
        $result = Invoke-ReviewPr -PullRequest '$null' -Head $script:Sha -HeadTimeoutSeconds 0

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'No pull request'
        Get-Log | Should -BeNullOrEmpty
    }

    It 'GivenPushedHeadNeverReachesThePullRequest_WhenRunWithHead_ThenFailsAndMarksThatHeadFailed' {
        # When
        $result = Invoke-ReviewPr -PullRequest "[pscustomobject]@{ number = 7; headRefOid = 'old'; baseRefName = 'master'; isDraft = `$false; state = 'OPEN' }" -Head $script:Sha -HeadTimeoutSeconds 0

        # Then
        $result.ExitCode | Should -Be 1
        $reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        Join-Path $reviews "7-$script:Sha.failed" | Should -Exist
        @(Get-Log | Where-Object { $_ -like 'review *' }).Count | Should -Be 0
    }

    It 'GivenOpenPullRequest_WhenRun_ThenPromptAsksForAFixedVerdictWhenThereAreNoFindings' {
        # Given: the authoring session repeats fixes until a review of the head has no findings (ADR 0014)

        # When
        Invoke-ReviewPr | Out-Null

        # Then
        Get-Content "$script:Log.prompt" -Raw | Should -Match '`No findings\.`'
    }
}

Describe 'wait-for-review.ps1' {
    BeforeAll {
        $script:Wait = Join-Path $PSScriptRoot '..\wait-for-review.ps1' | Resolve-Path | Select-Object -ExpandProperty Path

        # Runs wait-for-review.ps1 against $script:Repo for its head commit. -Sleep stands in for the pause between
        # checks; its body may write the review files, as review-pr.ps1 would in the background.
        function Invoke-Wait(
            [string] $PullRequest = '[pscustomobject]@{ number = 7; isDraft = $false; state = ''OPEN'' }',
            [string] $Sleep = '',
            [int] $TimeoutSeconds = 60) {
            $body = @"
& '$script:Wait' -RepoRoot '$script:Repo' -TimeoutSeconds $TimeoutSeconds -PollSeconds 1 ``
    -GetPullRequest { $PullRequest } ``
    -Sleep { param(`$Seconds) $Sleep }
exit `$LASTEXITCODE
"@
            Invoke-Wrapper $body
        }

        function Write-Posted { Set-Content -Path "$script:Reviews\7-$script:Sha.md" -Value 'No findings.'; Set-Content -Path "$script:Reviews\7-$script:Sha" -Value 'now' }
    }

    BeforeEach {
        $script:Repo = Join-Path $TestDrive "repo-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Path $script:Repo | Out-Null
        git -C $script:Repo init -q
        git -C $script:Repo -c user.name=t -c user.email=t@t commit -q --allow-empty -m init
        $script:Sha = (git -C $script:Repo rev-parse HEAD).Trim()
        $script:Reviews = Join-Path (git -C $script:Repo rev-parse --path-format=absolute --git-common-dir) 'claude-reviews'
        New-Item -ItemType Directory -Force -Path $script:Reviews | Out-Null
    }

    It 'GivenReviewOfHeadPosted_WhenWaiting_ThenPrintsItAndExitsZero' {
        # Given
        Write-Posted

        # When
        $result = Invoke-Wait

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'No findings\.'
    }

    It 'GivenHeadMovedAfterThePush_WhenWaitingForThePushedHead_ThenPrintsItsReview' {
        # Given: the session committed again while the background push ran
        $pushed = $script:Sha
        Set-Content -Path "$script:Reviews\7-$pushed.md" -Value 'Review of the pushed head.'
        Set-Content -Path "$script:Reviews\7-$pushed" -Value 'now'
        git -C $script:Repo -c user.name=t -c user.email=t@t commit -q --allow-empty -m later

        # When
        $result = Invoke-Wrapper @"
& '$script:Wait' -RepoRoot '$script:Repo' -Head '$pushed' -TimeoutSeconds 0 ``
    -GetPullRequest { [pscustomobject]@{ number = 7; isDraft = `$false; state = 'OPEN' } } -Sleep { param(`$Seconds) }
exit `$LASTEXITCODE
"@

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'Review of the pushed head\.'
    }

    It 'GivenReviewOfHeadPostedLater_WhenWaiting_ThenWaitsAndPrintsIt' {
        # When
        $result = Invoke-Wait -Sleep "Set-Content -Path '$script:Reviews\7-$script:Sha.md' -Value 'Finding: off by one.'; Set-Content -Path '$script:Reviews\7-$script:Sha' -Value now"

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'Finding: off by one\.'
    }

    It 'GivenReviewOfHeadFailsWhileWaiting_WhenWaiting_ThenExitsNonZero' {
        # When
        $result = Invoke-Wait -Sleep "Set-Content -Path '$script:Reviews\7-$script:Sha.failed' -Value now"

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'review-pr\.ps1'
    }

    It 'GivenReviewOfHeadFailedBeforeTheWait_WhenWaiting_ThenExitsNonZeroAtOnce' {
        # Given: the run for this push failed within seconds, before the session started the wait
        Set-Content -Path "$script:Reviews\7-$script:Sha.failed" -Value 'now'

        # When
        $result = Invoke-Wait -Sleep 'throw ''waited'''

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Not -Match 'waited'
        $result.Output | Should -Match 'review-pr\.ps1'
    }

    It 'GivenNoReviewArrives_WhenTimeoutPasses_ThenExitsNonZero' {
        # When
        $result = Invoke-Wait -TimeoutSeconds 0

        # Then
        $result.ExitCode | Should -Not -Be 0
    }

    It 'GivenDefaults_WhenRead_ThenTheWaitOutlastsTheHeadWaitOfReviewPr' {
        # Given: the hook starts review-pr.ps1 -Head when the push starts, and the session starts the wait after the
        # push; the review may begin only when review-pr.ps1's head wait ends and then takes minutes
        $default = {
            param($Script, $Name)
            $ast = [System.Management.Automation.Language.Parser]::ParseFile($Script, [ref]$null, [ref]$null)
            [int]($ast.ParamBlock.Parameters | Where-Object { $_.Name.VariablePath.UserPath -eq $Name }).DefaultValue.Extent.Text
        }

        # When
        $wait = & $default $script:Wait 'TimeoutSeconds'
        $headWait = & $default $script:ReviewPr 'HeadTimeoutSeconds'

        # Then: room for at least a 20-minute review after the longest head wait
        $wait | Should -BeGreaterOrEqual ($headWait + 1200)
    }

    It 'GivenNoPullRequestOnGitHub_WhenWaitingWithTheDefaultLookup_ThenExitsZeroWithoutWaiting' {
        # Given
        $bin = New-GhWithoutPullRequest

        # When
        $result = Invoke-Wrapper @"
`$env:PATH = '$bin;' + `$env:PATH
& '$script:Wait' -RepoRoot '$script:Repo' -Sleep { throw 'waited' }
exit `$LASTEXITCODE
"@

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'no review to wait for'
    }

    It 'GivenGhFailsForAnotherReason_WhenWaitingWithTheDefaultLookup_ThenFailsRatherThanReportNothingToWaitFor' {
        # Given: gh offline or logged out; ending cleanly would let the session ask the owner to review unreviewed work
        $bin = New-GhWithoutPullRequest 'error connecting to api.github.com'

        # When
        $result = Invoke-Wrapper @"
`$env:PATH = '$bin;' + `$env:PATH
& '$script:Wait' -RepoRoot '$script:Repo' -Sleep { throw 'waited' }
exit `$LASTEXITCODE
"@

        # Then
        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Not -Match 'no review to wait for'
        $result.Output | Should -Match 'error connecting'
    }

    It 'GivenPullRequest<Case>_WhenWaiting_ThenExitsZeroWithoutWaiting' -ForEach @(
        @{ Case = 'Missing'; PullRequest = '$null' },
        @{ Case = 'Draft'; PullRequest = '[pscustomobject]@{ number = 7; isDraft = $true; state = ''OPEN'' }' }
    ) {
        # When
        $result = Invoke-Wait -PullRequest $PullRequest -Sleep 'throw ''waited'''

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Not -Match 'waited'
    }
}

Describe 'claude-review-hook.ps1' {
    BeforeEach {
        $script:Log = Join-Path $TestDrive "log-$([guid]::NewGuid().ToString('N')).txt"
    }

    BeforeAll {
        # Runs the hook with a PostToolUse payload for a shell command; StartReview appends to the log instead.
        function Invoke-Hook([string] $Command, [string] $Raw, [switch] $AsReviewer, [string] $Cwd = 'C:\work\wt') {
            $payload = if ($Raw) { $Raw } else { @{ tool_name = 'Bash'; cwd = $Cwd; tool_input = @{ command = $Command } } | ConvertTo-Json -Compress }
            $reviewer = if ($AsReviewer) { "`$env:MAYDAY_CLAUDE_REVIEWER = '1'" } else { "Remove-Item Env:MAYDAY_CLAUDE_REVIEWER -ErrorAction SilentlyContinue" }
            $body = @"
$reviewer
& '$script:Hook' -StartReview { param(`$Directory, `$Head) Add-Content -Path '$script:Log' -Value "started `$Directory"; if (`$Head) { Add-Content -Path '$script:Log' -Value "head `$Head" } }
exit `$LASTEXITCODE
"@
            Invoke-Wrapper $body $payload
        }
    }

    It 'GivenCommand<Command>_WhenHookRuns_ThenStartsAReview' -ForEach @(
        @{ Command = 'gh pr create --base master --title x' },
        @{ Command = 'git push -u origin feature' },
        @{ Command = 'cd repo && git push 2>&1 | tail -5' },
        @{ Command = 'git -C ../wt push origin feature' },
        @{ Command = 'git -c core.hooksPath=.githooks push' }
    ) {
        # When
        $result = Invoke-Hook $Command

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -Be @('started C:\work\wt')
    }

    It 'GivenPushFromARepository_WhenHookRuns_ThenStartsTheReviewOfItsHead' {
        # Given: the hook runs when a background push starts, before the pull request has the pushed head
        $repo = Join-Path $TestDrive "repo-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Path $repo | Out-Null
        git -C $repo init -q
        git -C $repo -c user.name=t -c user.email=t@t commit -q --allow-empty -m init
        $sha = (git -C $repo rev-parse HEAD).Trim()

        # When
        $result = Invoke-Hook 'git push' -Cwd $repo

        # Then: the review and the session's wait both follow the pushed head, even if HEAD moves before the wait starts
        Get-Log | Should -Be @("started $repo", "head $sha")
        ($result.Output | ConvertFrom-Json).hookSpecificOutput.additionalContext | Should -Match "-Head $sha"
    }

    It 'GivenEarlierFailureMarkedForThePushedHead_WhenHookRuns_ThenClearsItBeforeTheReviewStarts' {
        # Given: an earlier push of the same commit failed and its review marked the head failed; the run this push
        # starts is detached and may not clear the mark before the session's wait reads it
        $repo = Join-Path $TestDrive "repo-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Path $repo | Out-Null
        git -C $repo init -q
        git -C $repo -c user.name=t -c user.email=t@t commit -q --allow-empty -m init
        $sha = (git -C $repo rev-parse HEAD).Trim()
        $reviews = Join-Path $repo '.git\claude-reviews'
        New-Item -ItemType Directory -Path $reviews | Out-Null
        Set-Content -Path "$reviews\7-$sha.failed" -Value 'earlier'
        Set-Content -Path "$reviews\7-$('1' * 40).failed" -Value 'another head'

        # When
        Invoke-Hook 'git push' -Cwd $repo | Out-Null

        # Then
        "$reviews\7-$sha.failed" | Should -Not -Exist
        "$reviews\7-$('1' * 40).failed" | Should -Exist
    }

    It 'GivenPush_WhenHookRuns_ThenTellsTheSessionToWaitForTheReviewAndAddressIt' {
        # Given: Claude Code shows a PostToolUse hook's plain stdout only in the transcript, not to Claude; only
        # hookSpecificOutput.additionalContext reaches the session

        # When
        $result = Invoke-Hook 'git push'

        # Then
        $output = $result.Output | ConvertFrom-Json
        $output.hookSpecificOutput.hookEventName | Should -Be 'PostToolUse'
        $context = $output.hookSpecificOutput.additionalContext
        $context | Should -Match 'wait-for-review\.ps1'
        $context | Should -Match '-RepoRoot "C:\\work\\wt"'
        $context | Should -Match 'background'
        $context | Should -Match 'No findings'
        # The wait runs up to an hour (the head wait plus the review). The tool's limit must outlast the script's own
        # deadline, so a stalled review ends with the script's message on how to recover, not with the tool killing it.
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot '..\wait-for-review.ps1'), [ref]$null, [ref]$null)
        $waitSeconds = [int]($ast.ParamBlock.Parameters | Where-Object { $_.Name.VariablePath.UserPath -eq 'TimeoutSeconds' }).DefaultValue.Extent.Text
        $toolTimeout = [regex]::Match($context, 'timeout of (\d+) ms').Groups[1].Value
        $toolTimeout | Should -Not -BeNullOrEmpty
        [int64]$toolTimeout | Should -BeGreaterOrEqual (($waitSeconds + 300) * 1000)
    }

    It 'GivenCommand<Command>_WhenHookRuns_ThenStartsNothing' -ForEach @(
        @{ Command = 'git status --short' },
        @{ Command = 'gh pr view 43' },
        @{ Command = 'git push --dry-run origin feature' }
    ) {
        # When
        $result = Invoke-Hook $Command

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -BeNullOrEmpty
    }

    It 'GivenReviewerSession_WhenItPushes_ThenStartsNothing' {
        # When
        $result = Invoke-Hook 'git push' -AsReviewer

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -BeNullOrEmpty
    }

    It 'GivenMalformedInput_WhenHookRuns_ThenStartsNothingAndExitsZero' {
        # When
        $result = Invoke-Hook -Raw 'not json'

        # Then
        $result.ExitCode | Should -Be 0
        Get-Log | Should -BeNullOrEmpty
    }
}

Describe 'claude review wiring' {
    It 'GivenSettings_WhenPostToolUseHooksRead_ThenReviewHookRunsAfterShellCommands' {
        # When
        $hooks = (Get-Content (Join-Path $script:RepoRoot '.claude\settings.json') -Raw | ConvertFrom-Json).hooks.PostToolUse
        $review = $hooks | Where-Object { $_.hooks.command -match 'claude-review-hook\.ps1' }

        # Then
        $review | Should -Not -BeNullOrEmpty
        'Bash' | Should -Match "^($($review.matcher))$"
        'PowerShell' | Should -Match "^($($review.matcher))$"
    }

    It 'GivenDefaultReviewCommand_WhenInspected_ThenAllowsReadingLinearButNothingThatWrites' {
        # Given: CLAUDE.md asks for Linear issues and projects as links with ID and title taken from Linear, and the
        # review is posted as a comment, so the reviewer must be able to look them up, and only that
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:ReviewPr, [ref]$null, [ref]$null)
        $review = $ast.ParamBlock.Parameters | Where-Object { $_.Name.VariablePath.UserPath -eq 'Review' }

        # When
        # Every mention of the connector, including a server-wide rule (mcp__claude_ai_Linear) or a wildcard, which would
        # grant its write tools too
        $linear = @([regex]::Matches($review.DefaultValue.Extent.Text, 'mcp__claude_ai_Linear[\w*]*') | ForEach-Object { $_.Value })

        # Then: exactly these four read tools
        ($linear | Sort-Object) | Should -Be @(
            'mcp__claude_ai_Linear__get_issue', 'mcp__claude_ai_Linear__get_project',
            'mcp__claude_ai_Linear__list_issues', 'mcp__claude_ai_Linear__list_projects')
    }

    It 'GivenWorkflows_WhenListed_ThenNoneReviewsPullRequestsOnGitHub' {
        Join-Path $script:RepoRoot '.github\workflows\claude-review.yml' | Should -Not -Exist
    }
}
