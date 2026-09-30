# Mayday motion planning

## Responsibility

Plan hexapod movements and evaluate/learn inverse leg kinematics.

## Public APIs

`MaydayMotionPlanner`, `TrackingMaydayMotionPlanner`, `StepByStepLearningMaydayMotionPlanner`, inverse-kinematics input/output/evaluators.

## Key Concepts

Timed motion, stance, position loss, leg targets, learned kinematics.

## Dependencies

Mayday leg/structure types, `RobotDomain` motion/time, ML packages via `MaydayDomain`.

## Dependency Rules

Behavior chooses goals; planning transforms them without owning native port access.

## Invariants

Goals must respect leg reachability, joint limits, and units.

## Architectural Constraints

The enclosing project currently references `Dynamixel`; [trace this exception](../../knowledge/findings/mayday-domain-hardware-reference.md) before restructuring.

## Common Pitfalls

Do not train or drive a real leg as an ordinary unit test.

## Related ADRs

[Stable abstractions](../../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

[Hardware reference](../../knowledge/findings/mayday-domain-hardware-reference.md).

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
