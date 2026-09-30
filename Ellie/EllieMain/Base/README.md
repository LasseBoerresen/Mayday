# Ellie base

## Responsibility

Wrap Ellie's behavior controller and cancellation lifecycle.

## Public APIs

`Ellie` (`Start`, `Stop`); created by `EllieFactory`.

## Key Concepts

Robot start/stop delegates behavior and cancellation.

## Dependencies

`RobotDomain.Behavior` and an owned cancellation token source.

## Dependency Rules

Keep hardware setup in `EllieMain.Program`, not the robot wrapper.

## Invariants

Stop must cancel the same token passed to behavior.

## Architectural Constraints

Starting behavior can eventually drive hardware.

## Common Pitfalls

[Physical runs](../../../knowledge/pitfalls/physical-runs.md).

## Related ADRs

[Hardware boundary](../../../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
