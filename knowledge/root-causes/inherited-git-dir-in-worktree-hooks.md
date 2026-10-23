# Pushing from a linked worktree turned the shared repository bare

## Symptoms

Pushing `chore/local-gates` from the linked worktree `.claude/worktrees/code-review-pr-42-e1e632` failed the `pre-push` gate in its script-test step: 11 `gate.ps1` tests failed (32 passed), for example "Expected 1, but got 0" and "Expected 'robot=False' to be found in collection $null". The same tests passed when run directly from that worktree (43 of 43).

Afterwards every git command in every checkout of the repository, the main checkout included, failed with `fatal: this operation must be run in a work tree`. The shared `.git/config` held `core.bare = true`.

## Investigation

- Setting `GIT_DIR` to the worktree's absolute git directory and running `scripts/Tests/Gate.Tests.ps1` directly reproduced the failures (12 failed) without the push.
- The tests create a scratch repository per test with `git -C <scratch> init`, and `gate.ps1 -RepoRoot <scratch>` runs `git rev-parse` and `git ls-files` there. With `GIT_DIR` set, git ignores the working directory and uses `GIT_DIR`, so both act on the real repository.
- `git init` with `GIT_DIR` set and no `GIT_WORK_TREE` reinitializes that repository as bare. The worktree's git directory takes its `core` settings from the shared `.git/config`, so `core.bare = true` landed there and affected every checkout.
- Git sets `GIT_DIR` when it runs hooks. For a linked worktree the value is absolute, so it still points at the real repository after a script changes directory. Hypothesis, not verified: from the main checkout git passes a relative `.git`, which resolves inside each scratch repository, which would explain why earlier pushes of this branch passed.

## Root Cause

`gate.ps1` and every step it runs inherited the hook's `GIT_DIR`. The gate found its repository from `-RepoRoot` by changing directory, but an inherited `GIT_DIR` overrides the directory. So the script tests' scratch repositories were the real repository: the tests failed, and a scratch `git init` reinitialized the real repository as bare.

## Resolution

- `20f0c54`: right after `Set-Location $RepoRoot`, `gate.ps1` removes `GIT_DIR`, `GIT_WORK_TREE`, `GIT_INDEX_FILE`, `GIT_COMMON_DIR`, `GIT_PREFIX`, `GIT_OBJECT_DIRECTORY` and `GIT_ALTERNATE_OBJECT_DIRECTORIES`. The test `GivenGitHookEnvironmentForAnotherRepo_WhenRun_ThenGatesRepoRootAndHidesItFromSteps` failed before the change and passes after it.
- After the change, the same push from the linked worktree passed the Release gate (44 of 44 script tests) and `core.bare` stayed `false`.
- Recovery for a repository already affected: `git config --file <repo>/.git/config core.bare false`.

## Prevention

- A script that runs inside a git hook and works on a repository other than the one being pushed must clear git's location variables before calling git, or pass the repository explicitly.
- Run script tests from a linked worktree too, not only from the main checkout: the hook environment differs between them.
- A test that sets `GIT_DIR` must point it at a scratch repository only. Running the script tests directly with `GIT_DIR` set to the real repository reinitializes that repository as bare.

## Related Components

[scripts](../../scripts/README.md)

## Related Tasks

None recorded. The fix is part of pull request #42 (`chore/local-gates`), with the Stop-hook fix `65ceb7d`.

## Related Decisions

[ADR 0006](../../docs/adr/0006-run-quality-gates-locally.md), [ADR 0007](../../docs/adr/0007-infrastructure-code-is-tested-like-product-code.md), [lesson: infrastructure tests must pass inside the thing they test](../lessons-learned/infrastructure-tests-run-in-a-different-environment.md)
