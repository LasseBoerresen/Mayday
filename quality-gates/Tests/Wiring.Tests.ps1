#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
# Checks that the repository is wired the way quality-gates/install says: the config files in the repository root
# match their templates, the Claude Code hook in .claude/settings.json matches the snippet, the hook scripts exist,
# and no CA1502 suppression lacks a justification. Names follow the repo's Given/When/Then convention.

BeforeAll {
    $script:Install = Join-Path $PSScriptRoot '..\install' | Resolve-Path | Select-Object -ExpandProperty Path
    $script:RepoRoot = Join-Path $PSScriptRoot '..\..' | Resolve-Path | Select-Object -ExpandProperty Path

    function Get-PostToolUseHook([string] $Path) {
        $hooks = (Get-Content $Path -Raw | ConvertFrom-Json).hooks.PostToolUse
        $hooks | Where-Object { $_.hooks.command -match 'complexity-hook\.ps1' }
    }
}

Describe 'quality-gates wiring' {
    It 'Given<File>InstallTemplate_WhenComparedWithRepositoryRoot_ThenIdentical' -ForEach @(
        @{ File = 'CodeMetricsConfig.txt' }, @{ File = 'Directory.Build.props' }, @{ File = '.editorconfig' }
    ) {
        # When
        $template = (Get-Content (Join-Path $script:Install $File) -Raw) -replace '\r\n', "`n"
        $installed = (Get-Content (Join-Path $script:RepoRoot $File) -Raw) -replace '\r\n', "`n"

        # Then
        $installed | Should -Be $template
    }

    It 'GivenHooksSnippet_WhenComparedWithSettings_ThenPostToolUseHookIdentical' {
        # When
        $snippet = Get-PostToolUseHook (Join-Path $script:Install 'hooks.snippet.json')
        $settings = Get-PostToolUseHook (Join-Path $script:RepoRoot '.claude\settings.json')

        # Then
        $snippet | Should -Not -BeNullOrEmpty
        ($settings | ConvertTo-Json -Depth 10) | Should -Be ($snippet | ConvertTo-Json -Depth 10)
    }

    It 'GivenSettingsHooks_WhenCommandsResolved_ThenEveryScriptExists' {
        # When
        $commands = (Get-Content (Join-Path $script:RepoRoot '.claude\settings.json') -Raw | ConvertFrom-Json).hooks.PSObject.Properties.Value.hooks.command

        # Then
        $commands | Should -Not -BeNullOrEmpty
        foreach ($command in $commands) {
            $script = [regex]::Match($command, '\$CLAUDE_PROJECT_DIR/([^"]+\.ps1)').Groups[1].Value
            $script | Should -Not -BeNullOrEmpty
            Join-Path $script:RepoRoot $script | Should -Exist
        }
    }

    It 'GivenCSharpSources_WhenScannedForCA1502Suppressions_ThenEachHasAJustification' {
        # When
        $sources = Get-ChildItem $script:RepoRoot -Recurse -Filter *.cs |
            Where-Object { $_.FullName -notmatch '[\/](bin|obj)[\/]' }
        $pragmas = $sources | Select-String -Pattern 'pragma warning disable.*CA1502'
        $unjustified = $sources | Select-String -Pattern 'SuppressMessage\([^)]*CA1502' |
            Where-Object { $_.Line -notmatch 'Justification\s*=\s*"[^"]+' }

        # Then
        @($pragmas) | Should -BeNullOrEmpty
        @($unjustified) | Should -BeNullOrEmpty
    }
}
