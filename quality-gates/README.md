# quality-gates

## Responsibility

Portable code quality gates for Claude Code. Today: cyclomatic complexity, at most 12 per method, enforced by the .NET SDK analyzer CA1502 ([ADR 0008](../docs/adr/0008-cyclomatic-complexity-gate-with-ca1502.md)). The folder has no Mayday-specific paths; copy it into another .NET repository to reuse it.

## Public APIs

- `complexity-gate.ps1 [-ProjectRoot <dir>] [-Target <sln|csproj>] [-InitBaseline] [-UpdateBaseline]`: builds the target (default: the only `.sln`, else the only `.csproj`). Exit 0 when it builds; exit 2 with the CA1502 errors (method and complexity) or, for any other build failure, the tail of the build output. With a `complexity-baseline.json` it runs in baseline mode, see below.
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
- No baseline here: this repository has no violations. For a repository that does, use baseline mode below and never raise the threshold.
- A method that must exceed the limit uses `[SuppressMessage("Maintainability", "CA1502", Justification = "...")]` with a real reason. Generated code is not analyzed.
- Tests in `Tests` build a scratch project with the real `dotnet build` (gate), replace the gate with a fake (hook) and check the wiring: root config files equal `install/`, the settings hook equals the snippet, hook scripts exist, every CA1502 suppression has a justification. ([ADR 0007](../docs/adr/0007-infrastructure-code-is-tested-like-product-code.md))

## Related Records

[ADR 0008](../docs/adr/0008-cyclomatic-complexity-gate-with-ca1502.md), [ADR 0006](../docs/adr/0006-run-quality-gates-locally.md)

## Baseline mode (existing violations)

CA1502 has no baseline of its own, so `complexity-gate.ps1` adds a ratchet: "no worse than today".

1. In the target repository set `dotnet_diagnostic.CA1502.severity = warning` in `.editorconfig`, so plain builds pass while violations exist.
2. `complexity-gate.ps1 -InitBaseline` writes `complexity-baseline.json`: for each `relative/path.cs::MethodName` the complexities of its violations, highest first (one per overload). Commit it.
3. With that file present the gate tolerates the recorded violations. It exits 2 on a new violation (`NEW`) or on a recorded method that grew (`GREW`). It builds with `WarningsNotAsErrors=CA1502`, so `TreatWarningsAsErrors` projects still pass.
4. When a method improves or is fixed the gate says so. `-UpdateBaseline` records the lower value and drops fixed methods. It refuses, leaving the file untouched, when anything is new or worse, so the baseline can only go down.

Limits: line numbers are not recorded, so unrelated edits do not matter, but moving a method to another file or renaming it makes it look new. The checks are driven by the build's CA1502 warnings, so a project that does not compile has no baseline result. The suppression test and the root-equals-`install/` wiring tests apply as before.
