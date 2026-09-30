# ManualBehavior

## Responsibility

Translate manual/experimental Mayday behavior into planner goals.

## Public APIs

`TerminalPostureBehaviorController`, `BabyLegsBehaviorController`, `SwayBehaviorController`, `PostureCommand`.

## Key Concepts

Posture selection, terminal input, timed behavior commands.

## Dependencies

`MaydayDataAccess`, `MaydayDomain`, and `RobotDomain`.

## Dependency Rules

Behavior selects goals; motion planning and device communication belong to their own layers.

## Invariants

Preserve behavior cancellation and timed posture sequencing.

## Architectural Constraints

Avoid constructing concrete actuator hardware inside controllers; `Robots` composes dependencies.

## Common Pitfalls

[Physical runs](../knowledge/pitfalls/physical-runs.md); do not assume a behavior test is hardware-free.

## Related ADRs

[Stable abstractions](../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
