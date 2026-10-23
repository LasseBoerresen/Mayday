# Pitfall

## Description

Running a script inside a git hook that calls git on any repository other than the one git started the hook for, while it still has the hook's inherited `GIT_DIR` (and, for some hooks, `GIT_INDEX_FILE`). The script tests that `gate.ps1` runs from the `pre-push` hook do this: each test creates a scratch repository with `git init`.

## Why It Is Dangerous

`GIT_DIR` overrides the working directory, so `Set-Location` or `git -C` into another repository does not change which repository git uses. For a linked worktree git sets it to an absolute path, so it reaches the real repository from anywhere. A scratch `git init` then reinitializes the real repository as bare (`core.bare = true` in the shared `.git/config`), and every checkout stops working, not only the one that pushed. Reads go to the wrong repository too, so tests fail for reasons unrelated to the change. The user has seen this happen more than once; the [root cause](../root-causes/inherited-git-dir-in-worktree-hooks.md) records one occurrence in detail.

## Warning Signs

- `fatal: this operation must be run in a work tree` in a checkout that worked a moment ago; `git config --get core.bare` prints `true`.
- Script tests that pass when run directly but fail in the `pre-push` hook, especially from a worktree under `.claude/worktrees`.
- A hook script or test that runs `git init`, `git -C`, or `git` after changing directory.

## Preferred Approach

- At the start of a hook-run script, after deciding which repository it works on, clear git's location variables: `GIT_DIR`, `GIT_WORK_TREE`, `GIT_INDEX_FILE`, `GIT_COMMON_DIR`, `GIT_PREFIX`, `GIT_OBJECT_DIRECTORY`, `GIT_ALTERNATE_OBJECT_DIRECTORIES`. `gate.ps1` does this right after `Set-Location $RepoRoot` (`20f0c54`), so all its steps start clean.
- In a test that needs a hook-like environment, point `GIT_DIR` at a scratch repository only, never at the real one.
- Push from a linked worktree as well as from the main checkout when changing hook scripts: the hook environment differs between them.
- To recover: `git config --file <repo>/.git/config core.bare false`.

## Related Components

[scripts](../../scripts/README.md)
