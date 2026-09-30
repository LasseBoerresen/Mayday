# EllieMain

## Responsibility

Ellie's application, robot structure, steering motion planning, behavior, and concrete composition.

## Public APIs

`EllieFactory`; see [Base](Base/README.md), [Behaviors](Behaviors/README.md), [MotionPlanning](MotionPlanning/README.md), and [Structures](Structures/README.md).

## Key Concepts

Articulated steering, wheel driver, timed movement goals, terminal commands.

## Dependencies

`RobotDomain`, `Dynamixel`; concrete setup in `Program.cs`.

## Dependency Rules

Reuse shared contracts without introducing Ellie dependencies into `RobotDomain`.

## Invariants

`Program.cs` initializes a real bus; `DefaultEllieStructure.MoveAt`/`GetState` and some planner operations currently throw `NotImplementedException`.

## Architectural Constraints

Do not document unimplemented motion as operational or start hardware to test it.

## Common Pitfalls

[Physical runs](../../knowledge/pitfalls/physical-runs.md); planning and structure paths are incomplete.

## Related ADRs

[Stable abstractions](../../docs/adr/0001-stable-robot-abstractions.md), [hardware boundary](../../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
