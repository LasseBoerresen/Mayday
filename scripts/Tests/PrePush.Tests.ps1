#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
# Tests for scripts/pre-push.ps1.
#
# Each test gets a scratch git repository with a master branch and a feature branch, and runs the hook in a child
# PowerShell with a fake gate that records its arguments. Names follow the repo's Given/When/Then convention.

BeforeAll {
    $script:PrePush = Join-Path $PSScriptRoot '..\pre-push.ps1' | Resolve-Path | Select-Object -ExpandProperty Path

    function Invoke-PrePush([string[]] $RefLines) {
        $refs = ($RefLines | ForEach-Object { "'$_'" }) -join ', '
        $wrapper = Join-Path $TestDrive "wrapper-$([guid]::NewGuid().ToString('N')).ps1"
        Set-Content -Path $wrapper -Value @"
& '$script:PrePush' -RepoRoot '$script:Repo' -Upstream master -GateScript '$script:FakeGate' -RefLines @($refs)
exit `$LASTEXITCODE
"@
        $ErrorActionPreference = 'Continue'
        $output = & powershell -NoProfile -ExecutionPolicy Bypass -File $wrapper 2>&1 | ForEach-Object { "$_" } | Out-String
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output; GateRan = (Test-Path $script:GateLog) }
    }

    # git warns on stderr, about line endings for one; run inside the gate with redirected streams, that line would be
    # a terminating error under the gate's Stop preference.
    function New-Commit([string] $Path, [string] $Content = 'x') {
        $ErrorActionPreference = 'Continue'
        $full = Join-Path $script:Repo $Path
        New-Item -ItemType Directory -Force -Path (Split-Path $full) | Out-Null
        Set-Content -Path $full -Value $Content
        git -C $script:Repo add -A
        git -C $script:Repo commit -q -m "change $Path"
        git -C $script:Repo rev-parse HEAD
    }

    function Get-RefLine([string] $LocalSha, [string] $RemoteSha = ('0' * 40)) {
        "refs/heads/feature $LocalSha refs/heads/feature $RemoteSha"
    }
}

Describe 'pre-push.ps1' {
    BeforeEach {
        $script:Repo = Join-Path $TestDrive "repo-$([guid]::NewGuid().ToString('N'))"
        $script:GateLog = Join-Path $TestDrive "gate-$([guid]::NewGuid().ToString('N')).txt"
        $script:FakeGate = Join-Path $TestDrive "fake-gate-$([guid]::NewGuid().ToString('N')).ps1"
        Set-Content -Path $script:FakeGate -Value "Add-Content -Path '$script:GateLog' -Value (`$args -join ' '); exit 0"
        New-Item -ItemType Directory -Path $script:Repo | Out-Null
        git -C $script:Repo init -q -b master
        git -C $script:Repo config user.email 'test@example.com'
        git -C $script:Repo config user.name 'Test'
        New-Commit 'Code.cs' | Out-Null
        git -C $script:Repo checkout -q -b feature
    }

    It 'GivenNewBranchChangingOnlyDocumentation_WhenPushed_ThenPassesWithoutRunningTheGate' {
        # Given
        New-Commit 'README.md' | Out-Null
        New-Commit 'docs/adr/0001-x.txt' | Out-Null
        New-Commit 'knowledge/INDEX.txt' | Out-Null
        $sha = New-Commit 'Media/photo.png'

        # When
        $result = Invoke-PrePush (Get-RefLine $sha)

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'Documentation-only push'
        $result.GateRan | Should -BeFalse
    }

    It 'GivenNewBranchChangingCode_WhenPushed_ThenRunsThePhysicalGateInRelease' {
        # Given
        New-Commit 'README.md' | Out-Null
        $sha = New-Commit 'Code.cs' 'changed'

        # When
        $result = Invoke-PrePush (Get-RefLine $sha)

        # Then
        $result.GateRan | Should -BeTrue
        Get-Content $script:GateLog | Should -Match '-Configuration Release -Physical'
    }

    It 'GivenChangeToClaudeSettings_WhenPushed_ThenRunsTheGateBecauseHooksAreCode' {
        # Given
        $sha = New-Commit '.claude/settings.json' '{}'

        # When
        $result = Invoke-PrePush (Get-RefLine $sha)

        # Then
        $result.GateRan | Should -BeTrue
    }

    It 'GivenRemoteAlreadyHasCodeChange_WhenPushingOnlyDocumentationOnTop_ThenSkipsTheGate' {
        # Given: the remote branch already has the code change, so only the new commit is pushed
        $remote = New-Commit 'Code.cs' 'changed'
        $sha = New-Commit 'Notes.md'

        # When
        $result = Invoke-PrePush (Get-RefLine $sha $remote)

        # Then
        $result.GateRan | Should -BeFalse
    }

    It 'GivenOneDocumentationRefAndOneCodeRef_WhenPushedTogether_ThenRunsTheGate' {
        # Given
        $docs = New-Commit 'Notes.md'
        git -C $script:Repo checkout -q -b other master
        $code = New-Commit 'Other.cs'

        # When
        $result = Invoke-PrePush @((Get-RefLine $docs), "refs/heads/other $code refs/heads/other $('0' * 40)")

        # Then
        $result.GateRan | Should -BeTrue
    }

    It 'GivenRemoteBranchDeletion_WhenPushed_ThenSkipsTheGate' {
        # Given
        $deleted = "(delete) $('0' * 40) refs/heads/old $(git -C $script:Repo rev-parse master)"

        # When
        $result = Invoke-PrePush @($deleted)

        # Then
        $result.ExitCode | Should -Be 0
        $result.GateRan | Should -BeFalse
    }

    It 'GivenGateFails_WhenPushingCode_ThenFailsWithTheGatesExitCode' {
        # Given
        Set-Content -Path $script:FakeGate -Value 'exit 3'
        $sha = New-Commit 'Code.cs' 'changed'

        # When
        $result = Invoke-PrePush (Get-RefLine $sha)

        # Then
        $result.ExitCode | Should -Be 3
    }

    It 'GivenHookFile_WhenInspected_ThenRunsThisScript' {
        # Given
        $hook = Join-Path $PSScriptRoot '..\..\.githooks\pre-push'

        # When
        $content = Get-Content $hook -Raw

        # Then
        $content | Should -Match 'scripts/pre-push\.ps1'
    }
}
