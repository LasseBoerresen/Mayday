#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
# Tests for scripts/review-pr.ps1 and scripts/claude-review-hook.ps1 (ADR 0010).
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
        [switch] $Force) {
        $forceArg = if ($Force) { '-Force' } else { '' }
        $body = @"
& '$script:ReviewPr' -RepoRoot '$script:Repo' $forceArg ``
    -GetPullRequest { $PullRequest } ``
    -Review { $Review } ``
    -PostComment { param(`$Number, `$BodyFile) Add-Content -Path 'LOG' -Value "post `$Number `$(Get-Content `$BodyFile -Raw)" }
exit `$LASTEXITCODE
"@.Replace('LOG', $script:Log).Replace('SHA', $script:Sha)
        Invoke-Wrapper $body
    }

    function Get-Log { if (Test-Path $script:Log) { @(Get-Content $script:Log) } else { @() } }
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
}

Describe 'claude-review-hook.ps1' {
    BeforeEach {
        $script:Log = Join-Path $TestDrive "log-$([guid]::NewGuid().ToString('N')).txt"
    }

    BeforeAll {
        # Runs the hook with a PostToolUse payload for a shell command; StartReview appends to the log instead.
        function Invoke-Hook([string] $Command, [string] $Raw, [switch] $AsReviewer) {
            $payload = if ($Raw) { $Raw } else { @{ tool_name = 'Bash'; cwd = 'C:\work\wt'; tool_input = @{ command = $Command } } | ConvertTo-Json -Compress }
            $reviewer = if ($AsReviewer) { "`$env:MAYDAY_CLAUDE_REVIEWER = '1'" } else { "Remove-Item Env:MAYDAY_CLAUDE_REVIEWER -ErrorAction SilentlyContinue" }
            $body = @"
$reviewer
& '$script:Hook' -StartReview { param(`$Directory) Add-Content -Path '$script:Log' -Value "started `$Directory" }
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

    It 'GivenWorkflows_WhenListed_ThenNoneReviewsPullRequestsOnGitHub' {
        Join-Path $script:RepoRoot '.github\workflows\claude-review.yml' | Should -Not -Exist
    }
}
