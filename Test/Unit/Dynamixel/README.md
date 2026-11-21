# Dynamixel unit tests

## Overview

Unit tests of the `Dynamixel` actuator adapter
([production README](../../../Dynamixel/README.md)): unit conversions, the
`Driver`, `DynamixelJointFactory`, and `PeriodicallyBatchedJointDriver` (from
`RobotDomain.Motion`) running on a `Driver`. The bus is a Moq mock of
`CommunicationBus`, so no serial port is opened. The folder also holds the
`EchoJointDriver` test double.

## Interface

### `StepAngleTests`

Checks the conversion between XL430-W250 position steps (0 to 4095, 2048 the
center) and angles.
File: `StepAngleTests.cs`.

| Test | What it checks |
|---|---|
| `GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle` (theory) | `StepAngle.ToAngle` maps steps 0, 1024, 2048, 3072, 4095 to -0.5, -0.25, 0, 0.25 and 0.5 - 1/4096 revolutions. |
| `GivenAdjacentSteps_WhenToAngle_ThenDifferByOneCountOf360DegreesOver4096` | One step is 360/4096 degrees. |
| `GivenEveryValidStep_WhenToAngleThenToSteps_ThenReturnsTheSameStep` | `ToSteps(ToAngle(step))` returns `step` for every step 0 to 4095 (a loop: the iteration is the behavior). |
| `GivenAngleBetweenTwoSteps_WhenToSteps_ThenReturnsTheNearestStep` (inline theory) | `StepAngle.ToSteps` rounds to the nearest step. |
| `GivenLowestAngle_WhenToSteps_ThenReturnsStep0` | -0.5 revolutions is step 0. |
| `GivenAngleOutsideHalfRevolutionEitherSide_WhenToSteps_ThenThrows` (inline theory) | Angles below -0.5 or at and above 0.5 revolutions throw `ArgumentException`. |
| `GivenAngleJustBelowHalfRevolution_WhenToSteps_ThenReturnsHighestStep` | 0.5 - 1/4096 revolutions is step 4095. |

### `DynamixelRotationalSpeedTests`

Checks the conversion to the actuator's speed unit.
File: `DynamixelRotationalSpeedTests.cs`.

| Test | What it checks |
|---|---|
| `Given_WhenDynamixelRotationalSpeedFrom1StepSpeed_ThenReturns1` | `DynamixelRotationalSpeed.FromRotationalSpeed(StepSize)` has `Value` 1. |

### `DriverTests`

Checks what `Driver.Initialize` writes to the Drive Mode register.
File: `DriverTests.cs`.

| Test | What it checks |
|---|---|
| `GivenDriveMode_WhenInitializeWithRotationDirection_ThenWritesExpectedDriveMode` (theory, `TheoryData<TestInput>`) | Whatever Drive Mode the servo reports, `Initialize` writes `0b0001` for `RotationDirection.Reverse` and `0b0000` for `Forward` to `ControlRegister.DriveMode`, once: the direction bit is set and the time-based-profile and torque-on-goal bits are cleared. |

### `DynamixelJointFactoryTests`

Checks that joints from `DynamixelJointFactory` delegate to the `JointDriver`.
File: `DynamixelJointFactoryTests.cs`.

| Test | What it checks |
|---|---|
| `GivenMockAdapter_WhenCreateJoint_ThenAdapterInitializeCalledWithJointId` | `New(...)` calls `JointDriver.Initialize` with the joint's `JointId` and rotation direction. |
| `GivenAdapter_WhenCreateJointAndCallSetGoal_ThenAdapterSetGoalCalledOnce` | `SetAngleGoal` on the created joint calls `JointDriver.SetGoalAngleFor` with the same id and goal. |

### `PeriodicallyBatchedJointDriverTests`

Checks that `PeriodicallyBatchedJointDriver` initializes actuators through the
`Driver`, writes goals from its periodic loop, and hands loop errors to the
`FatalErrorHandler`.
File: `PeriodicallyBatchedJointDriverTests.cs`.

| Test | What it checks |
|---|---|
| `Given_WhenInitialize_ThenCallsCommunicationBusTorqueEnableWithValue1` | `Initialize` writes 1 to `ControlRegister.TorqueEnable`, once. |
| `Given_WhenInitialize_ThenCallsCommunicationBusWriteVelocityLimit` | `Initialize` writes `ControlRegister.ProfileVelocity`, once. |
| `GivenInitializedJoint_WhenSetGoalAngle_ThenBatchWriteContainsStepsForThatJoint` | After `SetGoalAngleFor` with 30°, a batch write to `ControlRegister.GoalPosition` eventually (within 1 s) holds `StepAngle.ToSteps(30°)` for the joint. |
| `GivenBusReadThrows_WhenDriverRuns_ThenFatalErrorHandlerReceivesTheException` | An exception from the bus's batch `Read` in the loop reaches the injected `RecordingFatalErrorHandler`. |

### `EchoJointDriver` (helper)

A hardware-free `JointDriver` whose joints report the target of their last
goal as their angle.
File: `EchoJointDriver.cs`.

| Member | What it does |
|---|---|
| Constructor `(state)` | Sets the `JointState` every joint starts with. |
| `SetGoalAngleFor(id, goal)` | Stores `goal` as the joint's `AngleGoal` and its `Target` as `Angle`. |
| `GetState(id)` | Returns the stored state, or the initial state for a joint not yet seen. |
| `Initialize(id, rotationDirection)`, `Dispose()` | Do nothing. |

[Test/CLAUDE.md](../../CLAUDE.md) and the Dynamixel `CLAUDE.md` point to it for
hardware-free checks; no test in the repository uses it today.

## Invariants

- No test opens a port: every `Driver` gets a `Mock<CommunicationBus>`. Every
  test that calls `Initialize` sets its `Ping` to return `true`, so
  `Initialize` does not reboot. The `failingBusMock` of
  `GivenBusReadThrows_WhenDriverRuns_ThenFatalErrorHandlerReceivesTheException`
  sets up only the batch `Read`; that test never calls `Initialize`.
- `PeriodicallyBatchedJointDriver` starts its loop in its constructor, so the
  test class sets up every bus read the loop and `Initialize` use (batch
  `Read` returns an empty dictionary, single `Read` returns
  `StepAngle.StepCenter`), and disposes the driver after each test. An
  unconfigured read would throw on the loop thread
  ([pitfall](../../../knowledge/pitfalls/periodic-loops-in-tests.md)).
- Goal writes are asserted "eventually" with a bounded `SpinWait.SpinUntil`,
  not "once": the loop repeats the write every period.
- The goal under test (30°) differs from the initial goal (center step), so
  the test can tell them apart.
- Physical tests must not use `RecordingFatalErrorHandler`; these unit tests
  do.

## Side Effects

- `PeriodicallyBatchedJointDriverTests` runs a 1 ms loop on
  `TimeProvider.System` through `PeriodicScheduler`. The first scheduler in the
  test process sets the process priority to `High` and requests a 1 ms Windows
  timer resolution for the rest of the run (see
  [RobotDomain time tests](../RobotDomain/Time/README.md#side-effects)).
- No serial port, file or hardware access. `EchoJointDriver` keeps its state in
  memory.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.Dynamixel"`
