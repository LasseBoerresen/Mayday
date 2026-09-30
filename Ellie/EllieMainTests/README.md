# EllieMainTests

## Responsibility

Ellie end-to-end tests in `EndToEnd`.

## Public APIs

`EllieTests` and test extensions; not a production API.

## Key Concepts

Exercises Ellie composition and motion behavior.

## Dependencies

`EllieMain`, `Dynamixel`, and the shared `Test` project.

## Dependency Rules

Keep tests outside Ellie production code; inspect hardware usage before running.

## Invariants

Unimplemented Ellie structure/planning methods remain unimplemented; tests must not imply otherwise.

## Architectural Constraints

Avoid running tests that initialize physical devices without explicit confirmation.

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
