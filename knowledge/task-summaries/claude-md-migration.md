# Migrate agent guidance to CLAUDE.md

## Problem

Agent guidance lived in a single 184-line `AGENTS.md`. Claude Code loads `CLAUDE.md` automatically but not `AGENTS.md`, so the guidance did not reliably reach the agent in use. The file also loaded in full every session regardless of the task, listed the removed `MauiApp1` project, and enforced hardware safety only through prose. The knowledge workflow required updating every record category on every significant change, more ceremony than tasks usually warranted.

## Solution

- Renamed `AGENTS.md` to `CLAUDE.md` in a pure rename commit, so `git log --follow` keeps its history, and updated links in current docs.
- Reduced the root `CLAUDE.md` to about 100 lines of rules every task needs, and moved area-specific guidance into nested `CLAUDE.md` files in `Test`, `Dynamixel`, `RobotDomain` and `Ellie`. These load only when work touches the directory, and they point to component READMEs instead of restating them.
- Added `.claude/settings.json` with `ask` permission rules for `dotnet run`, `dotnet user-secrets set` and launching the `Main`/`EllieMain` binaries, and `allow` rules for build, test and read-only git commands.
- Added the model-invoked `record-knowledge` skill (`.claude/skills/record-knowledge`). It classifies evidence into record types, proposes records for approval, then writes them and updates the index, directory READMEs and component READMEs.

## Decisions Made

- Claude Code is the only agent tool in use, so `AGENTS.md` was renamed rather than kept as a shared file imported by `CLAUDE.md`.
- The knowledge workflow is triggered by real discoveries, decisions, diagnosed failures, abandoned approaches or significant completed work, and the user approves records before they are written.
- Commit messages use `Problem:` / `Rationale:` / `Impact:`. `Rationale:` replaces `Reason:`, which read ambiguously as the cause of the problem rather than why a solution was chosen. Earlier commits keep `Reason:`.
- Refactoring commits come before, and separate from, behavior commits. Knowledge records get their own `docs:` commit.
- The `Internal` namespace rule applies to all projects and lives in the root file.

## Tradeoffs

- Nested files are less discoverable than one document; the root file lists them to compensate.
- All `dotnet run` invocations prompt, not only the two robot projects, because reliably matching project paths across Bash and PowerShell is fragile.
- Physical tests self-skip unless `MAYDAY_ROBOT_IS_CONNECTED` is `True` (in Test user secrets or the environment). Allowing `dotnet test` relies on that flag staying `False`, which the `user-secrets set` ask rule protects; an environment variable set outside Claude Code is not covered.

## Follow-up Work

- After a week or two of use, review which `CLAUDE.md` rules agents actually needed or violated, and prune or sharpen them.
- Refine `record-knowledge` based on the records actually produced with it.
- Consider a hook that refuses `dotnet test` when `MAYDAY_ROBOT_IS_CONNECTED` is `True` in the environment.

## Related ADRs

[0001](../../docs/adr/0001-stable-robot-abstractions.md), [0002](../../docs/adr/0002-hardware-adapter-boundary.md) (now referenced from the nested `CLAUDE.md` files; unchanged)

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.
