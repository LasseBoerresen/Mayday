# RobotDomain motion unit tests

## Overview

Unit tests of motion plans in `RobotDomain.Motion`
([production README](../../../../RobotDomain/Motion/README.md)). The folder
holds one test of `LinearStepsMotionPlan<TState>`. The tests of
`PeriodicallyBatchedJointDriver`, which is also in `RobotDomain.Motion`, live
in [Test/Unit/Dynamixel](../../Dynamixel/README.md) because they run it on the
Dynamixel `Driver`.

## Interface

### `LinearStepsMotionPlanTests`

Checks which step a plan returns for a point in time.
File: `LinearStepsMotionPlanTests.cs`.

| Test | What it checks |
|---|---|
| `GivenPlanWithOneStep__WhenAtTimeAfter__ThenReturnThatOneStep` | A plan with one step, `Timed<int>.Passed(42)`, returns that step from `At(DateTimeOffset.UtcNow)`, a time after its arrival. |

## Invariants

- The test uses the wall clock (`DateTimeOffset.UtcNow`) only as "some time
  after the step's arrival"; it does not depend on how much time passes.

## Side Effects

- None.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.RobotDomain.Motion"`
