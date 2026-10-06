# Cyclomatic complexity gate with CA1502

## Status

Accepted

## Context

Claude Code produces code quickly, and nothing stopped it from producing methods that are hard to read and test. The owner wants quality gates that run during a session and at its end, starting with cyclomatic complexity at a threshold of 12, in a form reusable in other projects. [ADR 0006](0006-run-quality-gates-locally.md) already runs the build and tests locally through `scripts\gate.ps1`. At threshold 12 the current solution has no violations (measured by building with the rule enabled).

## Decision

Enable the Roslyn rule CA1502 as a build error: threshold in `CodeMetricsConfig.txt`, severity in `.editorconfig`, registered by `Directory.Build.props`. Package the scripts, install files and tests in a self-contained `quality-gates/` folder with no Mayday-specific paths; its Pester tests therefore live in `quality-gates/Tests`, a deliberate deviation from the `scripts/Tests` location in [ADR 0007](0007-infrastructure-code-is-tested-like-product-code.md), and `scripts\gate.ps1` runs both directories. Wiring tests keep the repository root files, the hook entry in `.claude/settings.json` and the install templates from drifting apart. `complexity-hook.ps1` runs after each Claude edit of a `.cs` file (a build, about 8 seconds here) and `complexity-gate.ps1` runs the same check at the end. Because the severity is `error`, `scripts\gate.ps1` already enforces the limit; the scripts are the early warning. Suppressions need a written justification. There is no baseline file, because nothing violates the limit.

## Alternatives Considered

- NDepend: richer (trends, dependency rules) but licensed per machine and not usable on GitHub runners; kept for later architecture rules.
- Lizard or other source scanners: faster per file but count differently from the compiler and add a second tool.
- SonarAnalyzer S1541: default threshold 10, brings many unrelated rules.
- A baseline or `GlobalSuppressions.cs`: CA1502 has no baseline, and suppressions let a method grow unnoticed. Not needed until a violation exists; a ratchet script would be the answer then.
- Fewer or no in-session checks (Stop only): feedback arrives after Claude has moved on from the file.

## Why

CA1502 adds no tool: it runs in the build everyone already has, with the same counting in the IDE, the hook and the gate.

## Consequences

- Benefits: new complexity is rejected where it is written; the folder can be copied to another repository.
- Costs: each `.cs` edit triggers a build of about 8 seconds; the hook builds the whole solution because Roslyn needs a compile.
- Limitations: CA1502 counts branches and `case` labels, not nesting (cognitive complexity); the wiring in `.claude/settings.json` is exercised by use, not tested; baseline mode matches methods by file and name, so moving or renaming one looks new (see the amendment).

## Related Components

[quality-gates](../../quality-gates/README.md), [scripts](../../scripts/README.md)

## Related ADRs

[0006](0006-run-quality-gates-locally.md), [0007](0007-infrastructure-code-is-tested-like-product-code.md)

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Amendment: baseline mode

The decision above assumed no existing violations, true for this repository, and left a baseline for when one is needed. Repositories adopting the folder will have violations, so `complexity-gate.ps1` now has a baseline (ratchet) mode: a committed `complexity-baseline.json` of per-method complexities, failing on new or worse violations and lowered only through `-UpdateBaseline`. The rule stays CA1502 and the threshold stays 12; the baseline only tolerates what already exists. It reads the CA1502 warnings from the build output (a shared SARIF error log would be overwritten by each project of a solution) and keys methods by file and name without line numbers, so overloads are matched by position among the file's violations of that name. See [quality-gates](../../quality-gates/README.md#baseline-mode-existing-violations).
