# Unit tests

## Responsibility

Fast focused tests for Mayday and shared collaborators.

## Public APIs

Tests, fakes, and test object mothers; no production API.

## Key Concepts

Arrange observable behavior with fake joints and controllable inputs.

## Dependencies

Production projects through `Test.csproj`; xUnit, Moq, and AwesomeAssertions.

## Dependency Rules

Do not move test-specific helpers into production projects for convenience.

## Invariants

Keep ordinary unit tests independent of real devices.

## Architectural Constraints

Use existing helpers and `TimeProvider` when timing affects tests.

## Common Pitfalls

[Physical runs](../../knowledge/pitfalls/physical-runs.md).

## Related ADRs

[Hardware boundary](../../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

None recorded.

## Related Root Causes

[Interpolation drift](../../knowledge/root-causes/interpolation-baseline-drift.md).

## Related Lessons Learned

[Goal versus measured state](../../knowledge/lessons-learned/goal-versus-measured-state.md).

## Related Failed Attempts

[Measured-angle interpolation](../../knowledge/failed-attempts/measured-angle-interpolation.md).
