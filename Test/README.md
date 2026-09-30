# Test

## Responsibility

Mayday and shared-layer xUnit unit/integration tests and test utilities.

## Public APIs

`Unit`, `Integration`, and `Utilities` include test mothers, fake joints, terminal, and physical-test attributes.

## Key Concepts

Hardware-independent tests by default; physical tests explicitly marked `PhysicalRobotFact`/`PhysicalRobotTheory`.

## Dependencies

References Mayday runtime, domain, adapter, behavior, and utility projects.

## Dependency Rules

Test helpers may depend on production code; production code must not depend on test projects.

## Invariants

Do not treat a passing skipped physical test as evidence of real-hardware behavior.

## Architectural Constraints

Prefer deterministic unit/integration tests and `TimeProvider`; never run physical tests casually.

## Common Pitfalls

[Physical runs](../knowledge/pitfalls/physical-runs.md).

## Related ADRs

[Hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

None recorded.

## Related Root Causes

[Interpolation baseline drift](../knowledge/root-causes/interpolation-baseline-drift.md).

## Related Lessons Learned

[Goal versus measured state](../knowledge/lessons-learned/goal-versus-measured-state.md).

## Related Failed Attempts

[Measured-angle interpolation](../knowledge/failed-attempts/measured-angle-interpolation.md).
