# Dynamixel integration tests

## Overview

Tests of the [Dynamixel](../../../Dynamixel/README.md) adapter against the
real servos: they open the FTDI serial adapter through
`NativeSerialPortCommunicationBus` and read or write servo registers. Every
test here is physical (`Tier=Physical`); none runs without the robot.

## Interface

### `NativeSerialPortCommunicationBusTests`

Writes goal positions to each servo and reads them back over the real bus.
File: `NativeSerialPortCommunicationBusTests.cs`. Test subject:
`NativeSerialPortCommunicationBus`.

| Test | What it checks |
|---|---|
| `GivenRobotIsAttached_WhenSetTwoGoalAnglesAndSleep1sBetween_ThenCurrentPositionWithin20` (`[PhysicalRobotFact]`) | For servo ids 1 to 18 in turn: enables torque, writes `GoalPosition` = `StepAngle.StepCenter - 100`, waits 0.1 s, reads `GoalPosition` back and asserts it is within 30 steps; then the same for `StepAngle.StepCenter`. |

The name does not match the code: it waits 0.1 s, not 1 s, the tolerance is
30 steps, not 20, and it reads back the `GoalPosition` register (the command),
not the servo's present position, so it checks the register round trip, not
that the servo reached the angle.

### `RotationDirectionOnRobotTests`

Checks that the rotation direction each servo stores matches the Mayday model.
Read-only. File: `RotationDirectionOnRobotTests.cs`.

| Test | What it checks |
|---|---|
| `GivenRobot_WhenReadDriveMode_ThenDirectionBitMatchesModel` (`[PhysicalRobotTheory]`, 18 rows: every `MaydayLegId.AllLegIds` x joints 1 to `MaydayLeg.JointCount`) | Reads the servo's `ControlRegister.DriveMode` and asserts bit 0 (`0` normal, `1` reverse) equals `Dynamixel.RotationDirection.FromDomain(...)` of the direction the model gives that joint. Writes each row's Drive Mode to the test output. |

| Helper | What it does |
|---|---|
| `TestInput(TestId Id, MaydayLegId MaydayLegId, int JointNumber)` | One theory row; `Id` is `<Side><SidePosition>-<joint>`. |
| `RecordingJointFactory` (private) | A `JointFactory` that records the `RotationDirection` asked for per `JointId` and returns echo joints from `EchoJointFactory`. |
| `ModelRotationDirectionOf` (private) | Builds the leg with `MaydayLegFactory`, the recording factory and the neutral map from [MaydayDomain/Base](../MaydayDomain/Base/README.md), and returns the recorded direction. |

## Invariants

- Both tests skip unless `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE` is `True` in the Test
  user secrets or the environment (`TestConfiguration.IsRobotClearToMove`).
  They do not check that the robot is connected: without the adapter,
  `NativeSerialPortCommunicationBus.CreateInitialized` fails with
  `Failed to open device with SN 'FT2H2YMW'`.
- `RotationDirectionOnRobotTests` never writes, enables torque or calls
  `Driver.Initialize`, so it reports the Drive Mode the servos hold before an
  initialization changes it. It opens the bus lazily, on first use, and
  disposes it in `Dispose` (closing the port).
- `NativeSerialPortCommunicationBusTests` opens the bus in a field initializer
  and never disposes it: the adapter stays taken until the test process ends
  ([root cause](../../../knowledge/root-causes/physical-tests-leak-the-serial-adapter.md),
  [MAY-81: Need: Mayday physical tests release the robot's port, so they all pass in one test process](https://linear.app/mayday-robot/issue/MAY-81)). Run its method in
  its own process, as `gate.ps1 -Physical` does.
- The model side of `RotationDirectionOnRobotTests` comes from
  `MaydayLegFactory`, so it checks the convention of
  [ADR 0013](../../../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md).

## Side Effects

- Both open the FTDI adapter (serial number `FT2H2YMW`), set its latency and
  open its COM port at 4 Mbit/s.
- `NativeSerialPortCommunicationBusTests` moves all 18 servos: each turns
  about 100 steps (about 8.8 degrees) from center and back, one at a time. It
  enables torque on each and leaves it enabled.
- `RotationDirectionOnRobotTests` reads one register per servo and writes to
  the xUnit test output; it does not move the robot.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Integration.Dynamixel"`.
  Both tests are `Tier=Physical`: `NativeSerialPortCommunicationBusTests`
  moves the robot. Run only after the operator confirms the robot, port and
  operating conditions. None of the tests here is hardware-free.
