#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
# Tests for scripts/quarantine-ratchet.ps1.
#
# Each test gets a scratch git repository whose master holds a small test file, then commits a change on a feature
# branch and runs the ratchet against master in a child PowerShell. Names follow the repo's Given/When/Then convention.

BeforeAll {
    $script:Ratchet = Join-Path $PSScriptRoot '..\quarantine-ratchet.ps1' | Resolve-Path | Select-Object -ExpandProperty Path

    $script:MasterTests = @'
using Xunit;

namespace Test.Unit;

[TestSubject(typeof(Leg))]
public class LegTests(ITestOutputHelper output)
{
    [Fact]
    public void GivenLeg_WhenMove_ThenMoves()
    {
        // Given
        var leg = new Leg();
    }

    [Fact]
    [Quarantine("Assertion mismatch, known (MAY-1).")]
    void GivenLeg_WhenTurn_ThenTurns()
    {
    }
}
'@

    function Set-TestFile([string] $Content, [string] $Path = 'Test/Unit/LegTests.cs') {
        $full = Join-Path $script:Repo $Path
        New-Item -ItemType Directory -Force -Path (Split-Path $full) | Out-Null
        Set-Content -Path $full -Value $Content
    }

    function Save-Commit {
        git -C $script:Repo add -A
        git -C $script:Repo commit -q -m change
    }

    function Invoke-Ratchet {
        $ErrorActionPreference = 'Continue'
        $output = & powershell -NoProfile -ExecutionPolicy Bypass -File $script:Ratchet -RepoRoot $script:Repo -Upstream master 2>&1 |
            ForEach-Object { "$_" } | Out-String
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output }
    }
}

Describe 'quarantine-ratchet.ps1' {
    BeforeEach {
        $script:Repo = Join-Path $TestDrive "repo-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Path $script:Repo | Out-Null
        git -C $script:Repo init -q -b master
        git -C $script:Repo config user.email 'test@example.com'
        git -C $script:Repo config user.name 'Test'
        Set-TestFile $script:MasterTests
        Save-Commit
        git -C $script:Repo checkout -q -b feature
    }

    It 'GivenBranchChangesNoMarks_WhenChecked_ThenHolds' {
        # Given
        Set-TestFile ($script:MasterTests -replace 'var leg = new Leg\(\);', 'var leg = new Leg(1);')
        Save-Commit

        # When
        $result = Invoke-Ratchet

        # Then
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'ratchet holds'
    }

    It 'GivenExistingTestGainsQuarantine_WhenChecked_ThenFailsNamingIt' {
        # Given
        Set-TestFile ($script:MasterTests -replace '(\[Fact\]\r?\n    public void GivenLeg_WhenMove)', "[Quarantine(`"Broken (MAY-2).`")]`n    `$1")
        Save-Commit

        # When
        $result = Invoke-Ratchet

        # Then: even a reason with an issue does not let an existing test into quarantine
        $result.ExitCode | Should -Be 1
        $result.Output | Should -Match 'LegTests\.GivenLeg_WhenMove_ThenMoves .*existed on master and is now quarantined'
    }

    It 'GivenExistingTestGainsSkip_WhenChecked_ThenFailsNamingIt' {
        # Given
        Set-TestFile ($script:MasterTests -replace '\[Fact\](\r?\n    public void GivenLeg_WhenMove)', '[Fact(Skip = "later")]$1')
        Save-Commit

        # When
        $result = Invoke-Ratchet

        # Then
        $result.ExitCode | Should -Be 1
        $result.Output | Should -Match 'LegTests\.GivenLeg_WhenMove_ThenMoves .*is now skipped'
    }

    It 'GivenExistingClassGainsQuarantine_WhenChecked_ThenFailsNamingTheClass' {
        # Given
        Set-TestFile ($script:MasterTests -replace '\[TestSubject', "[Quarantine(`"All broken.`")]`n[TestSubject")
        Save-Commit

        # When
        $result = Invoke-Ratchet

        # Then
        $result.ExitCode | Should -Be 1
        $result.Output | Should -Match 'LegTests \(.*existed on master and is now quarantined'
    }

    It 'GivenNewTestQuarantinedWithIssue_WhenChecked_ThenHolds' {
        # Given: the reason spans lines, as long reasons do
        Set-TestFile ($script:MasterTests -replace '(?s)\}\s*$', @'

    [Fact]
    [Quarantine("Assertion mismatch on the new expectation, "
        + "fixed by MAY-3.")]
    void GivenLeg_WhenLift_ThenLifts()
    {
    }
}
'@)
        Save-Commit

        # When
        $result = Invoke-Ratchet

        # Then
        $result.ExitCode | Should -Be 0
    }

    It 'GivenNewTestQuarantinedWithoutIssue_WhenChecked_ThenFails' {
        # Given
        Set-TestFile ($script:MasterTests -replace '(?s)\}\s*$', @'

    [Fact]
    [Quarantine("Fails, will look at it [someday].")]
    void GivenLeg_WhenLift_ThenLifts()
    {
    }
}
'@)
        Save-Commit

        # When
        $result = Invoke-Ratchet

        # Then
        $result.ExitCode | Should -Be 1
        $result.Output | Should -Match 'LegTests\.GivenLeg_WhenLift_ThenLifts .*new test quarantined without a MAY- issue'
    }

    It 'GivenQuarantineRemoved_WhenChecked_ThenHolds' {
        # Given
        Set-TestFile ($script:MasterTests -replace '    \[Quarantine\("Assertion mismatch, known \(MAY-1\)\."\)\]\r?\n', '')
        Save-Commit

        # When
        $result = Invoke-Ratchet

        # Then
        $result.ExitCode | Should -Be 0
    }

    It 'GivenQuarantinedTestMovedToAnotherFile_WhenChecked_ThenHolds' {
        # Given
        Remove-Item (Join-Path $script:Repo 'Test/Unit/LegTests.cs')
        Set-TestFile $script:MasterTests 'Test/Integration/LegTests.cs'
        Save-Commit

        # When
        $result = Invoke-Ratchet

        # Then
        $result.ExitCode | Should -Be 0
    }

    It 'GivenSkipAssignedInAConstructorBody_WhenChecked_ThenIsNotATestMark' {
        # Given: physical test attributes set Skip in their constructors; that is not a skipped test
        Set-TestFile -Path 'Test/Utilities/RobotFactAttribute.cs' -Content @'
public sealed class RobotFactAttribute : FactAttribute
{
    public RobotFactAttribute()
    {
        if (!TestConfiguration.IsRobotClearToMove())
            Skip = TestConfiguration.MaydayRobotIsNotClearToMoveReason;
    }
}
'@
        Save-Commit

        # When
        $result = Invoke-Ratchet

        # Then
        $result.ExitCode | Should -Be 0
    }

    It 'GivenUncommittedQuarantine_WhenChecked_ThenHoldsBecauseOnlyCommitsArePushed' {
        # Given
        Set-TestFile ($script:MasterTests -replace '(\[Fact\]\r?\n    public void GivenLeg_WhenMove)', "[Quarantine(`"x`")]`n    `$1")

        # When
        $result = Invoke-Ratchet

        # Then
        $result.ExitCode | Should -Be 0
    }
}
