# Physical pre-push gate (MAY-80)

## Problem

The gate excluded physical tests, and physical tests skip themselves without the robot, so no push ever checked Mayday and a run without it looked green. Skipping them by hand had become the habit. Some physical tests failed, and several tests were skipped permanently ([MAY-80](https://linear.app/mayday-robot/issue/MAY-80)).

## Solution

Two pull requests, merged 2026-10-25:

- **PR #50, rename and baseline.**
  - The robot flag became `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE`. CONTEXT.md now separates **Connected** (the port opens) from **Clear-to-move** (the operator's statement).
  - A run on the robot set the baseline. The leg-transform theory was quarantined (MAY-82). The scheduler timing test got a real-clock assertion and was quarantined (MAY-83).
  - Skipped tests were resolved: one became a manual test, two empty ones were deleted, and the map builder was marked `Tier=Manual`.
- **PR #51, the gate.**
  - A documentation-only push runs nothing.
  - Any other push runs `gate.ps1 -Physical`: quarantine ratchet, clear-to-move flag, connected check, build, physical test listing, then each physical method in its own process.
  - It fails on a failure, a skip, a quarantined test that passes, or a run with no test.
  - The Stop hook stays hardware-free.

## Decisions Made

- [ADR 0011](../../docs/adr/0011-pre-push-gate-drives-the-robot.md): the pre-push gate drives the robot, and the operator's flag is the standing confirmation. CLAUDE.md's hard rule says so.
- Known failures are the `[Quarantine]` set ([ADR 0004](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md)). It may only shrink: existing tests may not gain a quarantine or skip, and new ones need a `MAY-` issue.
- The baseline landed in its own PR first, because the ratchet refuses the baseline's own quarantines.
- One process per physical method, because of the [adapter leak](../root-causes/physical-tests-leak-the-serial-adapter.md).

## Tradeoffs

A code push now needs Mayday attached and clear to move, and costs a `dotnet test` start per physical method. The ratchet reads attributes by pattern for speed, which needed many review rounds to harden ([lesson](../lessons-learned/ratchet-by-pattern-reading-has-a-long-tail.md)). Gaps left to review are listed in ADR 0011's limitations: a move to `Tier=Manual`, deletion, renames, a file renamed into `docs/`, unusual secrets-file contents, and the working tree standing in for the pushed commits.

## Follow-up Work

- [MAY-81](https://linear.app/mayday-robot/issue/MAY-81): physical tests release the port.
- [MAY-82](https://linear.app/mayday-robot/issue/MAY-82): leg-transform expectations in the planner's frame.
- [MAY-83](https://linear.app/mayday-robot/issue/MAY-83): scheduler period on the real clock.
- Speeding up slow physical tests and adding simulation stay separate issues.

## Related ADRs

[0004](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md), [0005](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md), [0006](../../docs/adr/0006-run-quality-gates-locally.md), [0009](../../docs/adr/0009-manual-robot-tests-confirmed-by-a-human.md), [0010](../../docs/adr/0010-local-claude-review-of-every-pull-request.md), [0011](../../docs/adr/0011-pre-push-gate-drives-the-robot.md)

## Related Findings

[Worktrees share the main checkout's hooks](../findings/absolute-hooks-path-shared-by-worktrees.md)

## Related Root Causes

[Physical tests leak the robot's serial adapter](../root-causes/physical-tests-leak-the-serial-adapter.md), [User secrets override environment variables in tests](../root-causes/user-secrets-override-environment.md)

## Related Lessons Learned

[A ratchet that reads C# by pattern has a long tail of bypasses](../lessons-learned/ratchet-by-pattern-reading-has-a-long-tail.md), [Infrastructure tests must pass inside the thing they test](../lessons-learned/infrastructure-tests-run-in-a-different-environment.md)
