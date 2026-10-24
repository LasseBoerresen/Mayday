# scripts

## Responsibility

The local quality gate and the hooks that enforce it ([ADR 0006](../docs/adr/0006-run-quality-gates-locally.md)).

## Public APIs

- `gate.ps1 [-Configuration Release|Debug] [-Quarantined] [-Force]`: builds `Mayday.sln`, runs the `Test` project without `[Quarantine]` tests, then the Pester tests of these scripts. Exits non-zero on the first failing step. A pass is remembered per configuration for an unchanged working tree (every non-Markdown file git tracks or could track, excluding `.idea`, `.claude`, `knowledge`, `docs`, `Media`) and skipped next time unless `-Force`. It finds the repository from `-RepoRoot` only: it clears inherited git location variables such as `GIT_DIR`, which git sets when it runs hooks of a linked worktree, before any step runs.
- `claude-stop-gate.ps1`: the Claude Code `Stop` hook. Runs `gate.ps1` in Debug, in its own process; exits 2 with the tail of the output on any failure, a crash included, so Claude must fix it before finishing.
- `quality-gates/` (outside this directory): the portable complexity gate; `gate.ps1` also runs its Pester tests. See its [README](../quality-gates/README.md).
- `review-pr.ps1 [-PullRequest <n>] [-Force]`: has a separate headless `claude -p` session review the pull request (default: the current branch's) in a temporary worktree of its head commit and posts the output with `gh pr comment`. Skips drafts, closed pull requests and head commits already reviewed (markers in `<git common dir>/claude-reviews`). Advisory ([ADR 0010](../docs/adr/0010-local-claude-review-of-every-pull-request.md)).
- `claude-review-hook.ps1`: the Claude Code `PostToolUse` hook on `Bash|PowerShell`. After `gh pr create` or `git push`, starts `review-pr.ps1` for the command's working directory (the payload's `cwd`) in a hidden background process, logging to its own `review-*.log` in `<git common dir>/claude-reviews`, and always exits 0. git options before `push` (`git -C ../wt push`) are recognized.
- `pre-push.ps1`: the `pre-push` hook. Reads the pushed refs from standard input and passes a documentation-only push (every changed file is Markdown or under `docs/`, `knowledge/`, `Media/`) without building or testing; any other push runs `gate.ps1` in Release. A ref is compared with the remote commit, or for a new branch with its merge-base with `origin/master`.
- `.githooks/pre-push` (outside this directory): runs `pre-push.ps1`. Enable with `git config core.hooksPath .githooks`.

## Dependencies

PowerShell 5.1, git, the .NET SDK and Pester 5+ (`Install-Module Pester -MinimumVersion 5.0 -Scope CurrentUser`). The review also needs `gh` and `claude`, both logged in.

## Invariants

- The gate forces `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE=False` and never runs `Main` or `EllieMain`.
- A failed run never records a pass.
- Scripts have tests in `Tests` ([ADR 0007](../docs/adr/0007-infrastructure-code-is-tested-like-product-code.md)). `gate.ps1` takes `-RepoRoot`, `-BuildCommand`, `-TestCommand` and `-ScriptTestCommand`, the Stop hook takes `-GateScript`, and `pre-push.ps1` takes `-RepoRoot`, `-Upstream`, `-GateScript` and `-RefLines`, so tests can replace the expensive steps; the defaults are the real behavior.

## Related Records

[ADR 0006](../docs/adr/0006-run-quality-gates-locally.md), [ADR 0007](../docs/adr/0007-infrastructure-code-is-tested-like-product-code.md), [Infrastructure tests must pass inside the thing they test](../knowledge/lessons-learned/infrastructure-tests-run-in-a-different-environment.md), [Pushing from a linked worktree turned the shared repository bare](../knowledge/root-causes/inherited-git-dir-in-worktree-hooks.md), [Git location variables inherited from hooks](../knowledge/pitfalls/git-env-inherited-from-hooks.md), [User secrets override environment variables in tests](../knowledge/root-causes/user-secrets-override-environment.md)
