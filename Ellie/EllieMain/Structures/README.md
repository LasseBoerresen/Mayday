# Ellie structures

## Responsibility

Represent Ellie's wheel configuration and state boundary to its planner.

## Public APIs

`EllieStructure`, `DefaultEllieStructure`, `State`.

## Key Concepts

Front wheel rotation directions and structure-level movement requests.

## Dependencies

`RobotDomain.Motion.WheelDriver` and Ellie motion types.

## Dependency Rules

Keep wheel-driver implementation replaceable; concrete bus construction belongs in startup.

## Invariants

`Start` initializes front left/right wheels once; `MoveAt` and `GetState` currently throw `NotImplementedException`.

## Architectural Constraints

Do not infer a complete physical movement API from the interface alone.

## Common Pitfalls

[Physical runs](../../../knowledge/pitfalls/physical-runs.md); initialization touches actuator driver.

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
