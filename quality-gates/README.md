# quality-gates

## Responsibility

Portable code quality gates for Claude Code. Today: cyclomatic complexity, at most 12 per method, enforced by the .NET SDK analyzer CA1502 ([ADR 0008](../docs/adr/0008-cyclomatic-complexity-gate-with-ca1502.md)). The folder has no Mayday-specific paths; copy it into another .NET repository to reuse it.

## Public APIs

- `complexity-gate.ps1 [-ProjectRoot <dir>] [-Target <sln|csproj>]`: builds the target (default: the only `.sln`, else the only `.csproj`). Exit 0 when it builds; exit 2 with the CA1502 errors (method and complexity) or, for any other build failure, the tail of the build output.
- `complexity-hook.ps1 [-GateScript] [-ProjectRoot]`: Claude Code `PostToolUse` hook. Reads the hook JSON on stdin, skips anything but `.cs` files, otherwise runs the gate and exits 2 with its message so Claude fixes the method while it is still in context.
- `install/`: files to copy into the target repository root.
  - `CodeMetricsConfig.txt`: the threshold (`CA1502: 12`).
  - `Directory.Build.props`: registers that file as an analyzer input.
  - `.editorconfig`: turns CA1502 (off by default) into a build error.
  - `hooks.snippet.json`: `PostToolUse` and `Stop` hook entries for `.claude/settings.json`.

## Install in another repository

1. Copy `quality-gates/` and the files in `install/` (the first three to the repo root; merge if they exist).
2. Merge `hooks.snippet.json` into `.claude/settings.json`.
3. Optionally add `quality-gates/Tests` to the repo's Pester run (Pester 5+).

## Dependencies

PowerShell 5.1, the .NET SDK (CA1502 ships in its analyzers) and Pester 5+ for the tests.

## Invariants

- The threshold has one source: `CodeMetricsConfig.txt`. The scripts never pass a threshold.
- The build itself fails above the threshold, so `dotnet build`, the IDE and [`scripts\gate.ps1`](../scripts/README.md) enforce it too; these scripts are the early warning, not the only check.
- Existing code has no violations, so there is no baseline. If one is ever needed, add a ratchet (fail on new or worse violations) rather than raising the threshold.
- A method that must exceed the limit uses `[SuppressMessage("Maintainability", "CA1502", Justification = "...")]` with a real reason. Generated code is not analyzed.
- Tests in `Tests` build a scratch project with the real `dotnet build` (gate), replace the gate with a fake (hook) and check the wiring: root config files equal `install/`, the settings hook equals the snippet, hook scripts exist, every CA1502 suppression has a justification. ([ADR 0007](../docs/adr/0007-infrastructure-code-is-tested-like-product-code.md))

## Related Records

[ADR 0008](../docs/adr/0008-cyclomatic-complexity-gate-with-ca1502.md), [ADR 0006](../docs/adr/0006-run-quality-gates-locally.md)
