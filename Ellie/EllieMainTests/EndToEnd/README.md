# Ellie end-to-end tests

## Overview

Tests that start a whole Ellie, assembled from the real
[EllieMain](../../EllieMain/README.md) behavior, motion planner and structure,
with only the `ActuatorDriver` mocked (Moq). They touch no hardware; two of
the three expect behavior the production code does not have yet.

## Interface

### `EllieTests`

Starts Ellie, optionally sends a terminal command, stops it and checks the
calls that reached the mocked actuator driver. File: `EllieTests.cs`.
Production types: `EllieFactory`, `Ellie`, `TerminalMovementBehaviorController`,
`ArticulatedSteeringEllieMotionPlanner`, `DefaultEllieStructure`,
`PeriodicallyBatchedWheelDriver`.

| Test | What it checks |
|---|---|
| `WhenCreateDefaultEllie__ThenIsNotNull` (`[Fact]`) | `EllieFactory.CreateDefault()` returns an `Ellie`. |
| `WhenStartEllie__ThenAllWheelsAreInitializedOnce` (`[Fact]`) | After a start-wait-stop of 1 s each, `ActuatorDriver.Initialize` was called once for `WheelId.FrontLeft` with `RotationDirection.Reverse` and once for `WheelId.FrontRight` with `Forward`. |
| `GivenNewlyStartedEllie__WhenSendAccelerateForwardMovementCommand__ThenAllWheelsSetToNonZeroForwardMotion` (`[Fact]`) | With `AccelerateForward` queued on the terminal, after a start-wait-stop, `ActuatorDriver.RotateAt` was called at least once for each front wheel with a speed above zero. |

| Member | What it does |
|---|---|
| `EllieFactory` (private property) | Builds, on each read, a new `EllieFactory` over a new `CancellationTokenSource`, `TimeProvider.System`, a `RecordingFatalErrorHandler` and the class's shared `TestTerminal` and `Mock<ActuatorDriver>`. |

A fourth test, for articulation joint initialization, is commented out.

### `EllieTestExtensions`

Runs Ellie briefly in a test. C# extension block on `Ellie`. File:
`EllieTestExtensions.cs`.

| Member | What it does |
|---|---|
| `StartWaitActWaitStopWait(waitTime, action = null)` | Starts `Ellie.Start` on a background task, waits `waitTime` (or until `Start` ends), invokes `action` and waits again if given, calls `Stop`, waits `waitTime`, and awaits the start task if it has completed, so an exception from `Start` fails the test. |

## Invariants

- No test constructs a `NativeSerialPortCommunicationBus`, `Driver` or
  `DynamixelJointFactory`; the only actuator is the loose `Mock<ActuatorDriver>`.
- `WhenStartEllie__ThenAllWheelsAreInitializedOnce` and the
  `AccelerateForward` test fail against the current code; they are the two
  failures ADR 0004 records for `EllieMainTests`, and they are not
  quarantined. In the code: nothing calls `EllieStructure.Start`, the only
  caller of `WheelDriver.Initialize`; `ArticulatedSteeringEllieMotionPlanner`'s
  plan step, once a goal is set, throws `NotImplementedException`
  (`MapBodyMotionToStructureMotions`) before reaching the structure; the
  `RecordingFatalErrorHandler` records it and the loop ends; and
  `PeriodicallyBatchedWheelDriver.RotateAt` throws `NotImplementedException`.
- `Start` returns as soon as the terminal runs out of input
  (`TestTerminal.ReadLine` returns `null`); `Stop` cancels the token of the
  planner's loop.
- Timing uses `TimeProvider.System` and `Task.Delay`. A start-wait-stop takes
  at most 2 × `waitTime` (3 × with an action), but only about one `waitTime`
  when `Start` returns early, as it does here: the terminal holds at most one
  command, so each run takes about 1 s.

## Side Effects

- `ArticulatedSteeringEllieMotionPlanner.Start` creates a `PeriodicScheduler`,
  whose static constructor raises the test process to
  `ProcessPriorityClass.High` and requests a 1 ms Windows timer resolution
  (`HighResolutionWindowsTimerSetting`). Its 1 s loop busy-waits
  (`Thread.SpinWait`) on a thread-pool thread until `Stop`.
- `TestTerminal` and `TerminalMovementBehaviorController` write the command
  list, prompts and goal updates to the console.

## Verification

- Run: `dotnet test Ellie\EllieMainTests\EllieMainTests.csproj --filter "FullyQualifiedName~EllieMainTests.EndToEnd"`.
  No test has a `Tier` trait; all are hardware-free. Two are expected to fail
  (above).
