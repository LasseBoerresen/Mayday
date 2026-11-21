# Robots

## Overview

Builds a runnable Mayday robot: it wires the Dynamixel joints, the default
hexapod structure, a motion planner and one behavior controller into a
`MaydayRobot` that can be started and stopped. `Main` calls it at startup and
the tests in `Test` call it to drive the real robot.

## Responsibility

Assemble Mayday structures, planners, behavior, and actuator factory into a runnable robot.

## Public APIs

`MaydayRobotFactory` and `Base.MaydayRobot` (`Start`, `Stop`).

## Interface

### `MaydayRobotFactory`

Creates Mayday robots, each with its own hardware connection. Class in
namespace `Robots`. File: `MaydayRobotFactory.cs`.

Constructor: `(LegPostureByPositionMap LegPostureByPositionMap,
FatalErrorHandler fatalErrorHandler)`. The map goes to every leg; the handler
goes to the joint driver and, where it takes one, the planner and behavior.

Every method returns an `Eff<MaydayRobot>`. Calling the method, before the
effect runs:

1. Creates a new `CancellationTokenSource`, which the robot will own.
2. Calls `DynamixelJointFactory.Create(cts, timeProvider, fatalErrorHandler)`,
   which already looks up the FTDI adapter (see Side Effects).

Running the effect then builds, in order:

1. The serial bus and the `DynamixelJointFactory`, whose
   `PeriodicallyBatchedJointDriver` starts writing at once.
2. A `MaydayLegFactory` over that joint factory and the map, and the default
   structure from `MaydayStructureFactory.CreateDefault()`, which creates and
   initializes every joint.
3. The planner and behavior controller in the table below.
4. A `MaydayRobot` over the controller and the token source.

The token source belongs to the call, not to a run. Running the same effect a
second time opens the serial port again, which fails while the first robot
holds it (see Invariants), so it does not give a second robot.

| Method | Planner | Behavior controller | Called by |
|---|---|---|---|
| `CreateWithTerminalPostureBehaviorController(Terminal, TimeProvider)` | `TrackingMaydayMotionPlanner` | `TerminalPostureBehaviorController`, reading commands from the `Terminal` | Tests only |
| `CreateWithBabyLegsBehaviorController(TimeProvider)` | `StepByStepLearningMaydayMotionPlanner` with `InverseLegKinematicsNeuralNetwortTensorflowNetImpl` | `BabyLegsBehaviorController` | `Main` (`Train`), tests |
| `CreateWithSwayBehavior(TimeProvider)` | `TrackingMaydayMotionPlanner` | `SwayBehaviorController` | `Main` (`Run`), tests |

The behaviors are described in [ManualBehavior](../ManualBehavior/README.md).

### `MaydayRobot`

A composed robot: starts its behavior and stops it. Class in namespace
`Robots.Base`. File: `Base/MaydayRobot.cs`.

Constructor: `(BehaviorController behaviorController, CancellationTokenSource
cancelTokenSource)`.

| Member | What it does |
|---|---|
| `Unit Start()` | Calls `behaviorController.Start()`: runs the behavior on the calling thread and returns when it ends. |
| `void Stop()` | Cancels the token source. Returns at once; it does not wait for `Start()` to return. |

`MaydayRobot` is not `IDisposable` and disposes nothing.

## Key Concepts

Concrete composition, cancellation token ownership, alternative behavior configurations.

## Dependencies

`ManualBehavior` directly and its transitive domain/adapter/data-access
dependencies. The code uses `Dynamixel` (`DynamixelJointFactory`),
`MaydayDomain` (`LegPostureByPositionMap` and the leg, structure and planner
types), `RobotDomain` (`BehaviorController`, `FatalErrorHandler`), `Generic`
(`Terminal`) and `LanguageExt` (`Eff`, `Unit`). `MaydayRobotFactory.cs` has a
`using MaydayDataAccess;` that no code uses. `Main` and `Test` reference this
project.

## Dependency Rules

Compose concrete objects here rather than in the shared domain.

## Invariants

- Each robot's `CancellationTokenSource` drives its
  `PeriodicallyBatchedJointDriver`, its behavior controller and, for the sway
  robot, the planner's tracking loop (`SwayBehaviorController` calls
  `motionPlanner.Start(ct)`; the terminal and baby-legs controllers do not
  start the planner). `Stop` cancels them all: the goal writes to the servos
  end within one 20 ms period, and the behavior ends after its current step,
  which for the terminal robot means after the next input line. It is not an
  emergency stop: the robot holds its last commanded position (see
  [ManualBehavior invariants](../ManualBehavior/README.md#invariants)).
- A stopped robot cannot be restarted: the token stays cancelled, so its joint
  driver no longer writes to the servos.
- Every run of a factory effect opens the serial port again and creates a new
  joint factory. The FTDI adapter has one owner at a time, so creating a
  second robot in the same process fails while the first still holds the port.
- Nothing disposes the `DynamixelJointFactory` or its serial bus: the port
  stays open until the process ends (tracked in
  [MAY-81](https://linear.app/mayday-robot/issue/MAY-81)). If the effect fails
  after the port opened, for example while initializing a joint, the port and
  the driver loop stay alive and no robot is returned through which to stop
  them.
- A failed adapter status or port setup is an `Error` of the effect, which
  `RunUnsafe()` throws. An exception thrown by the FTDI library during the
  eager adapter lookup is not caught and escapes the factory call itself.
- The structure is always `MaydayStructureFactory.CreateDefault()`.

## Side Effects

- Calling a factory method, before its effect runs, opens the FTDI adapter
  (serial number `FT2H2YMW`), requests 1 ms latency, reads its COM port name
  and closes it again, when the adapter is plugged in and free
  (`NativeSerialPortCommunicationBus.CreateInitialized`). The latency request
  does not change the COM port's latency (a `TODO` there).
- Running the effect opens the serial port and keeps it open, and starts the
  `PeriodicallyBatchedJointDriver` loop that writes goal angles every 20 ms,
  before any joint exists. The first `PeriodicScheduler` in the process raises
  it to `ProcessPriorityClass.High`. It then initializes every joint of the
  default structure through `Driver.Initialize`, which writes the actuators'
  configuration registers and enables torque (see
  [Dynamixel](../Dynamixel/README.md)). The port open and baud rate are logged
  to the console.
- `CreateWithBabyLegsBehaviorController` also builds and compiles a
  TensorFlow.NET model and prints its summary to the console.
- `MaydayRobot.Start()` moves the real robot, as its behavior controller
  describes, and blocks the calling thread.

## Architectural Constraints

Treat factory methods returning `Eff<MaydayRobot>` as potentially hardware-affecting.

## Common Pitfalls

- [Physical runs](../knowledge/pitfalls/physical-runs.md); do not call creation just to inspect configuration.
- Creating the effect is not hardware-free: it already opens and closes the
  FTDI adapter, even when the effect is never run.

## Verification

- Run: `dotnet build Robots\Robots.csproj`.
- `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.Robots"`
  runs `MaydayDayRobotFactoryTests`, which calls `CreateWithSwayBehavior` but
  never runs the returned effect, so its assertion inside `Map` never runs and
  it checks only that creating the effect does not throw. It opens the FTDI
  adapter when that is plugged in (see Side Effects).
- `Test.Integration.Main.MaydayRobotTests`, despite its namespace, tests
  `MaydayRobotFactory` and `MaydayRobot`: it starts the terminal and baby-legs
  robots on a background task, calls `Stop()` after 0.5 s and returns without
  waiting for `Start()`, which ends later. Its tests are `PhysicalRobotFact` and
  move the robot: run them only after the operator confirms the robot, port
  and operating conditions. No test runs `CreateWithSwayBehavior`'s effect.

## Related ADRs

[Hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

[Current domain reference](../knowledge/findings/mayday-domain-hardware-reference.md).

## Related Root Causes

[Physical tests leak the serial adapter](../knowledge/root-causes/physical-tests-leak-the-serial-adapter.md) (why the port stays open).

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
