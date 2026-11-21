# Main integration tests

## Overview

Physical tests that assemble Mayday the way the composition root does:
[Robots](../../../Robots/README.md) factories and
[MaydayDomain](../../../MaydayDomain/README.md) structures over real
[Dynamixel](../../../Dynamixel/README.md) joints. Despite the namespace, no
test here uses a type from [Main](../../../Main/README.md); every test moves
the robot.

## Interface

### `MaydayLegTests`

Moves the real legs through a `TrackingMaydayMotionPlanner` and checks link
transforms or asks the person watching. File: `MaydayLegTests.cs`. Production
types: `DynamixelJointFactory`, `MaydayLegFactory`, `MaydayStructureFactory`,
`TrackingMaydayMotionPlanner`.

| Test | What it checks |
|---|---|
| `GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected` (`[PhysicalRobotTheory]`, `[Quarantine]`, rows from `TestObjectFactory.DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected`) | Sets `MaydayLegPosture.Neutral` over 1 s, waits 1 s, and compares the LF leg's transform of each link from `GetTransformsOf` with the expected leg-frame transform. Quarantined ([MAY-82: Need: Mayday physical leg test checks link transforms in the frame the planner reports](https://linear.app/mayday-robot/issue/MAY-82)): `GetTransformsOf` returns body-frame transforms. |
| `GivenTipsAtStanceWidth_WhenMoveAllTipsDownThenUp_ThenAllTipsMoveVertically` (`[ManualRobotFact]`) | Sets all tips with `SetTipPositionsForLegs` to (0.125, 0, z) m, in 1 cm steps of 0.125 s: z from 0.18 m down to about −0.21 m, then from −0.22 m up to about 0.17 m (each loop stops before its bound), then asserts the human confirms all six tips moved straight down about 40 cm and back up. |

### `MaydayRobotTests`

Starts and stops a whole Mayday robot. File: `MaydayRobotTests.cs`. Test
subject: `MaydayRobot`, built with `TestObjectMother.MaydayRobotFactory`
from [Robots/Base](../Robots/Base/README.md).

| Test | What it checks |
|---|---|
| `GivenMayWithTerminalPostureBehaviorController_WhenStartThenSleepThenStop_ThenSucceeds` (`[PhysicalRobotFact]`) | `CreateWithTerminalPostureBehaviorController` with an empty `TestTerminal`, `Start` on a background task, 0.5 s wait, `Stop`: no exception reaches the test. |
| `GivenMayWithBabyLegsBehaviorController_WhenStartThenSleepThenStop_ThenSucceeds` (`[PhysicalRobotFact]`) | The same with `CreateWithBabyLegsBehaviorController`. |

Neither has an assertion: an exception thrown inside the background `Start`
is not observed by the test.

## Invariants

- Physical tests skip unless `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE` is `True` in the
  Test user secrets or the environment; the manual test also needs
  `MAYDAY_HUMAN_IS_PRESENT` = `True`. Neither checks that the robot is
  connected.
- Both classes use the neutral map (`LegPostureByPositionMap` of
  [MaydayDomain/Base](../MaydayDomain/Base/README.md)), which finds a posture
  only for the origin. The manual tip test therefore depends on posture
  lookups the neutral map cannot answer
  ([pitfall](../../../knowledge/pitfalls/posture-map-silent-fallback.md)).
- Physical tests keep `FailFastFatalErrorHandler`: an exception in a periodic
  loop ends the test process.
- Nothing is disposed. `MaydayLegTests` holds its joint factory, structure and
  planner in `static readonly` fields, so the port stays open for the whole
  test process; each `MaydayRobotTests` test opens its own and never closes
  it. Run each physical method in its own process, as `gate.ps1 -Physical`
  does ([root cause](../../../knowledge/root-causes/physical-tests-leak-the-serial-adapter.md),
  [MAY-81: Need: Mayday physical tests release the robot's port, so they all pass in one test process](https://linear.app/mayday-robot/issue/MAY-81)).
- No test returns the robot to a resting posture.

## Side Effects

- Opens the FTDI adapter and serial port, initializes every joint (torque
  enabled) and starts the 20 ms joint loop through a `PeriodicScheduler`
  (process priority `High`, 1 ms Windows timer resolution).
- Moves the robot: `MaydayLegTests` to `Neutral` or through the tip sweep.
  The terminal posture robot schedules `Sitting`, `SittingTall`, `Sitting`
  (1 s each); `Start` is not awaited, so this continues after the test
  returns. The baby-legs robot schedules `Neutral` over 1 s; the 0.5 s `Stop`
  cancels it before its kick loop.
- `CreateWithBabyLegsBehaviorController` builds and compiles a TensorFlow.NET
  model (`InverseLegKinematicsNeuralNetwortTensorflowNetImpl`) and prints its
  summary.
- `TestTerminal` writes to the console; `MaydayLegTests` writes to the test
  output; the manual test opens a PowerShell prompt window
  (`ConsoleWindowHumanPrompt`, 10 minute timeout).

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Integration.Main."`.
  All tests are `Tier=Physical` or `Tier=Manual` and move the robot: run only
  after the operator confirms the robot, port and operating conditions. None
  is hardware-free.
