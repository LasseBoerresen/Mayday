# Finding

## Description

On the development machine `core.hooksPath` is the absolute path `C:\Users\fiffi_m4jrsay\RiderProjects\Mayday\.githooks`. Linked worktrees share the repository's config, so every worktree runs the hooks of the main checkout, not its own `.githooks`. The hook scripts then run relative to the pushed worktree (`scripts/pre-push.ps1`, `scripts/gate.ps1`).

## Why It Matters

A branch that changes a hook never runs its own hook on its own pushes. Every push of PR #51 (MAY-80) ran master's old `pre-push`, which called the hardware-free `gate.ps1`, so the new physical pre-push gate was never exercised by a push before merge; it was verified by running `gate.ps1 -Physical` by hand instead. The new hook took effect for all worktrees only once the main checkout moved to the merged `master`. The reverse also holds: once the main checkout has the new hook, a branch cut before `scripts/pre-push.ps1` existed cannot push until it is rebased ([scripts README](../../scripts/README.md)).

## Evidence

`git config core.hooksPath` from the worktree printed the absolute path. Every PR #51 push printed the old gate's steps (`Tests, quarantined excluded`) and none of the physical steps. After the owner checked out `origin/master` in the main checkout, its `.githooks/pre-push` called `scripts/pre-push.ps1`.

## Recommendation

When changing a hook on a branch, verify it by running the hook's script directly; the push does not test it. A relative `core.hooksPath` (`git config core.hooksPath .githooks`, as CLAUDE.md says) would make each worktree run its own hooks; changing the machine's setting is the owner's call.

## Related Components

[scripts](../../scripts/README.md)

## Related Decisions

[ADR 0006](../../docs/adr/0006-run-quality-gates-locally.md), [ADR 0011](../../docs/adr/0011-pre-push-gate-drives-the-robot.md)
