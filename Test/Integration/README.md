# Integration tests

## Responsibility

Exercise Mayday component interactions across robot, domain, and adapter boundaries.

## Public APIs

Tests and test object mothers; no production API.

## Key Concepts

Mixed hardware-independent and explicitly marked physical tests.

## Dependencies

`Test.csproj` references Mayday runtime and collaborators.

## Dependency Rules

Keep integration fixtures test-local and check attributes before execution.

## Invariants

`PhysicalRobotFact`/`PhysicalRobotTheory` mark tests requiring a connected robot.

## Architectural Constraints

Do not equate skipped physical tests with successful hardware validation.

## Common Pitfalls

[Physical runs](../../knowledge/pitfalls/physical-runs.md).

## Related ADRs

[Hardware boundary](../../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
