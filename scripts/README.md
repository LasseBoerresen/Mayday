# scripts

## Responsibility

The local quality gate and the hooks that enforce it ([ADR 0006](../docs/adr/0006-run-quality-gates-locally.md)).

## Public APIs

- `gate.ps1 [-Configuration Release|Debug] [-Quarantined] [-Force]`: builds `Mayday.sln`, runs the `Test` project without `[Quarantine]` tests, then the Pester tests of these scripts. Exits non-zero on the first failing step. A pass is remembered per configuration for an unchanged working tree (every non-Markdown file git tracks or could track, excluding `.idea`, `.claude`, `knowledge`, `docs`, `Media`) and skipped next time unless `-Force`.
- `claude-stop-gate.ps1`: the Claude Code `Stop` hook. Runs `gate.ps1` in Debug; exits 2 with the tail of the output on failure so Claude must fix it before finishing.
- `.githooks/pre-push` (outside this directory): runs `gate.ps1` in Release. Enable with `git config core.hooksPath .githooks`.

## Dependencies

PowerShell 5.1, git, the .NET SDK and Pester 5+ (`Install-Module Pester -MinimumVersion 5.0 -Scope CurrentUser`).

## Invariants

- The gate forces `MAYDAY_ROBOT_IS_CONNECTED=False` and never runs `Main` or `EllieMain`.
- A failed run never records a pass.
- Scripts have tests in `Tests` ([ADR 0007](../docs/adr/0007-infrastructure-code-is-tested-like-product-code.md)). `gate.ps1` takes `-RepoRoot`, `-BuildCommand`, `-TestCommand` and `-ScriptTestCommand`, and the hook takes `-GateScript`, so tests can replace the expensive steps; the defaults are the real behavior.

## Related Records

[ADR 0006](../docs/adr/0006-run-quality-gates-locally.md), [ADR 0007](../docs/adr/0007-infrastructure-code-is-tested-like-product-code.md), [Infrastructure tests must pass inside the thing they test](../knowledge/lessons-learned/infrastructure-tests-run-in-a-different-environment.md)
