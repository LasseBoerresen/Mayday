# RobotDomain unit tests

## Overview

Unit tests and test doubles for the shared foundation `RobotDomain`
([production README](../../../RobotDomain/README.md)), one folder per
production namespace. They need no hardware and no robot structure. The
tests of `PeriodicallyBatchedJointDriver` (`RobotDomain.Motion`) live in
[Test/Unit/Dynamixel](../Dynamixel/README.md).

## Subdirectories & Boundaries

| Folder | Contents |
|---|---|
| [Geometry](./Geometry/README.md) | `QTests`, `TransformTests`, `XyzTests`: rotations, transform chaining and halfway interpolation, `Xyz` construction. |
| [Motion](./Motion/README.md) | `LinearStepsMotionPlanTests`: which step a plan returns at a time. |
| [Structures](./Structures/README.md) | No tests. `FakeJoint`, a `Joint` fixed at one angle, used by `MaydayLegTests`. |
| [Time](./Time/README.md) | `PeriodicSchedulerTests`: cadence under a fake clock and on the real clock, and fatal-error handling. |

No test here is `Tier=Physical` or `Tier=Manual`. `PeriodicSchedulerTests`
raises the test process's priority and Windows timer resolution (see
[Time](./Time/README.md#side-effects)).

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.RobotDomain"`
- Blocking run, without quarantined tests: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.RobotDomain&Quarantine!=true"`
