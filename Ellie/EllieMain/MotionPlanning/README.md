# Ellie motion planning

## Responsibility

Convert timed body movement goals into articulated-steering structure motions.

## Public APIs

`EllieMotionPlanner`, `ArticulatedSteeringEllieMotionPlanner`, `Motion`, `StructureSet`.

## Key Concepts

Forward speed, turning speed, timed plans, periodic plan execution.

## Dependencies

`EllieMain.Structures` and `RobotDomain` motion/time/geometry.

## Dependency Rules

Planner requests structure movement; it does not access Dynamixel native IO directly.

## Invariants

`MapBodyMotionToStructureMotions` and `TurnBy` are currently unimplemented.

## Architectural Constraints

Do not start a plan on real hardware until missing operations and safety behavior are defined.

## Common Pitfalls

[Physical runs](../../../knowledge/pitfalls/physical-runs.md); periodic execution can hit unimplemented code.

## Related ADRs

[Stable abstractions](../../../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
