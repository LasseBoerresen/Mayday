# MaydayDomain integration tests

## Overview

Tests of [MaydayDomain](../../../MaydayDomain/README.md) legs and structures,
most together with the robot's real posture map from
[MaydayDataAccess](../../../MaydayDataAccess/README.md); the link transform
test uses the neutral map, as its physical twin does. Most run on echo
joints without hardware; one manual test moves the real Mayday and asks the
person watching.

## Interface

### `DefaultMaydayStructureTests`

Moves the tips of an echo structure and checks where they and the body lean
end up, and which frame its link transforms are in. File:
`DefaultMaydayStructureTests.cs`. Production types:
`DefaultMaydayStructure`, `MaydayLegPosture`, `Thorax`, built with
`TestObjectMother.CreateEchoStructureWithDefaultMap()` or, for the link
transform test, `MaydayStructureFactory.CreateEcho()` (neutral map, as the
physical test uses).

| Test | What it checks |
|---|---|
| `GivenStructureWithStandingPostureAndTipsMovedBackward3cm__WhenGetCurrentLean__ThenIs3cmForward` (`[Fact]`) | From `StandingWide`, `MoveTipsBy` 3 cm backward (−X) gives a `GetCurrentLean` of 3 cm forward in X within 1 cm. Z is not checked. |
| `GivenSittingTall_WhenMoveAllTipsForward5cm_ThenTipMoves5cmForward` (`[Theory]`, one row per leg) | From `SittingTall`, `MoveTipsBy` 5 cm forward moves that leg's tip (`GetPositionsOf(LinkName.Tip)`) by 5 cm in X, within 1 cm. It is the echo prediction of the manual test below. |
| `GivenNeutral_WhenGetTransformsOf_ThenLeftFrontIsLegFrameTransformAtItsMount` (`[Theory]`, rows from `TestObjectFactory.DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected`) | On `MaydayStructureFactory.CreateEcho()` at `Neutral`, the LF transform of each link from `GetTransformsOf` is the body-frame transform `Thorax.LeftFrontTransform + <leg-frame row>`, within 1 cm and 0.01 rev. It is the echo prediction of the physical link transform test in [Main](../Main/README.md). |

`TestInput(TestId Id, MaydayLegId MaydayLegId)` is the theory row.

### `MaydayLegInverseKinematicsTests`

Sets a tip position on one echo leg and checks it is reached. File:
`MaydayLegInverseKinematicsTests.cs`. Production type: `MaydayLeg`, left
center, built with `TestObjectMother.CreateEchoLegWithDefaultMap`.

| Test | What it checks |
|---|---|
| `WhenSetReachableTipPositionTo_TheReachesThatPosition` (`[Fact]`) | After `SetTipPositionTo` (0.2, 0, 0) m, `GetTransformOf(LinkName.Tip).Xyz` is that position within 1 cm. |

### `MaydayStructureManualTests`

Moves the real Mayday and asks the person watching whether it moved as
expected. File: `MaydayStructureManualTests.cs`. Production types:
`DynamixelJointFactory`, `MaydayLegFactory`, `MaydayStructureFactory`, with
`DefaultLegPostureByPositionMap`.

| Test | What it checks |
|---|---|
| `GivenSittingTallRobot_WhenMoveAllTipsForward5cm_ThenAllTipsMoveForward` (`[ManualRobotFact]`) | Sets `SittingTall` over 2 s, asks the human to confirm Mayday sits tall with straight legs, moves all tips 5 cm forward over 2 s, and asserts the human confirms all six moved about 5 cm forward. |

Object mother: [Base](./Base/README.md).

## Invariants

- The hardware-free tests use `DefaultLegPostureByPositionMap`: the neutral
  map cannot place tips. The map places tips to about 1 cm
  ([finding](../../../knowledge/findings/posture-map-accuracy.md), [MAY-74: Need: Mayday has analytical inverse kinematics with sub-mm accuracy](https://linear.app/mayday-robot/issue/MAY-74)), so
  the moves are 3 cm and 5 cm against the 1 cm tolerance of
  `TestObjectFactory.AssertXyzEqual`.
- Each test builds its own structure or leg; only the loaded default map is
  shared.
- The manual test skips unless both `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE` and
  `MAYDAY_HUMAN_IS_PRESENT` are `True` (`TestConfiguration`). It starts from
  `SittingTall`, not `Sitting`, because from `Sitting` the map has no posture
  for the rear legs' forward targets. It moves only 5 cm: larger moves make
  the rear femur motor hit the body
  ([root cause](../../../knowledge/root-causes/rear-femur-hits-body.md)).
- In a `finally`, even after a failed assertion or a missing answer, the
  manual test sets `Sitting` and cancels its token; `using` disposes the
  `DynamixelJointFactory`, which stops the joint loop.
  `PeriodicallyBatchedJointDriver.Dispose` does not close the bus, so the
  adapter stays open until the test process ends.

## Side Effects

- All tests read `DefaultLegPostureByPositionMap.json` from the output
  directory, once per process.
- The manual test opens the FTDI adapter and serial port, initializes all 18
  joints (`Driver.Initialize` writes Drive Mode and enables torque), starts
  the 20 ms joint loop through a `PeriodicScheduler` (process priority
  `High`, 1 ms Windows timer resolution), and moves the robot: sitting tall,
  tips 5 cm forward, then sitting. It blocks on wall-clock sleeps of 3 s per
  move.
- It opens a PowerShell prompt window (`ConsoleWindowHumanPrompt`, Windows
  only, 10 minute timeout) for each question and writes temporary files for
  it.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Integration.MaydayDomain"`.
  This includes the `Tier=Manual` test, which moves the robot and needs a
  person at it.
- Hardware-free: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Integration.MaydayDomain&Tier!=Physical&Tier!=Manual"`.
