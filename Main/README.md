# Main

## Overview

The Mayday executable and its composition root. It loads the leg posture map,
reads the startup mode and starts the real robot with the behavior that mode
selects.

## Responsibility

Mayday executable startup and selection of the configured robot behavior.

## Public APIs

Executable entry point in `Program.cs`; `StartupMode` selects Run or Train.

## Interface

`Main` is an executable, not a library: nothing references its types except
`Test.csproj`, and no test uses them. Its interface is the process start, the
environment variable it reads and the launch profiles that set it.

### Startup

#### Entry point

Starts the robot and runs its behavior until the process ends. Top-level
statements in `Program.cs`. In order, it:

1. Loads `DefaultLegPostureByPositionMap.json` from `AppContext.BaseDirectory`
   with `LegPostureByPositionMapFileRepo.ForDefaultMapIn(...).Load()`, and
   creates a `MaydayRobotFactory` with that map and a
   `FailFastFatalErrorHandler`.
2. Reads `StartupMode` from the environment variable `mayday_startup_mode`.
3. Creates the robot for that mode (table below) with `TimeProvider.System`.
4. Calls `MaydayRobot.Start()` on the calling thread, inside
   `Run(Runtime.New())`, and throws the resulting `Error` with `ThrowIfFail()`.

| Mode | Factory method | Behavior |
|---|---|---|
| `Run` | `MaydayRobotFactory.CreateWithSwayBehavior` | `SwayBehaviorController` over a `TrackingMaydayMotionPlanner`. |
| `Train` | `MaydayRobotFactory.CreateWithBabyLegsBehaviorController` | `BabyLegsBehaviorController` over a `StepByStepLearningMaydayMotionPlanner` with `InverseLegKinematicsNeuralNetwortTensorflowNetImpl`. |

#### `StartupMode`

Names the behavior the executable starts. Enum in namespace `Main`:
`Run` (0), `Train` (1). File: `StartupMode.cs`.

#### `mayday_startup_mode`

The environment variable that selects the mode. Its value is parsed with
`Enum.TryParse<StartupMode>`, so it is case-sensitive and surrounding
whitespace is ignored.

| Value | Result |
|---|---|
| unset | Fails with `No startup mode specified in environment variable: mayday_startup_mode`. |
| `Run`, `Train` | Starts that mode. |
| a number | Parsed as the enum value: `0` starts `Run`, `1` starts `Train`, any other number fails with `Invalid startup mode: <number>`. |
| comma-separated names | Combined bitwise: `Run,Train` starts `Train`. |
| anything else | Fails with `Invalid startup mode: <value>`. |

### Non-code assets

| File | What it does |
|---|---|
| `Properties/launchSettings.json` | Launch profiles `Run` and `Train`, each setting `mayday_startup_mode` to its name. |
| `appsettings.json` | Logging levels. No code reads it, and `Main.csproj` does not copy it to the output. |

`DefaultLegPostureByPositionMap.json` lives in `MaydayDataAccess`, whose
project file copies it to the output directory of each referencing project.

## Key Concepts

`mayday_startup_mode`, `DefaultLegPostureByPositionMap.json`, `MaydayRobotFactory`.

## Dependencies

`Program.cs` uses `Robots` (`MaydayRobotFactory`, `MaydayRobot`),
`MaydayDataAccess` (`LegPostureByPositionMapFileRepo`, reached through
`Robots`' references, not a direct one), `RobotDomain`
(`FailFastFatalErrorHandler`) and the `LanguageExt.Sys` package.
`Main.csproj` also references the projects `Dynamixel`, `Generic`,
`ManualBehavior` and `MaydayDomain` and the package `JetBrains.Annotations`,
which no code in `Main` uses directly.

## Dependency Rules

Own concrete startup configuration; do not move startup IO into shared domains.

## Invariants

The map is loaded before the mode is read, and the mode is checked before any
hardware is touched: a missing or invalid mode fails without opening the serial
port. A missing, unparsable or empty map file throws from `Load` before the
mode is read.

The map is always read from the application base directory, never from the
working directory.

Every startup failure ends the process with an unhandled exception: `Error`
values through `ThrowIfFail()`, map-loading exceptions directly. The unused
local function `PrintErrorToConsole` is not called. Planned (a `TODO` in
`CreateMaydayRobotFactory`): turn a failed map read into an `Error` instead of
an exception.

Fatal errors in the robot's periodic tasks end the process
(`FailFastFatalErrorHandler` calls `Environment.FailFast`).

## Side Effects

- Reads `DefaultLegPostureByPositionMap.json` from disk.
- Both modes open the Dynamixel serial port through
  `DynamixelJointFactory.Create` and initialize every joint, which enables
  actuator torque. A port that cannot be opened fails startup.
- Both modes start a background loop in `PeriodicallyBatchedJointDriver` that
  writes the joints' goal angles every 20 ms. The first `PeriodicScheduler`
  raises the whole process to `ProcessPriorityClass.High` and requests the
  high-resolution Windows timer (`HighResolutionWindowsTimerSetting`).
- `Run` moves the robot: it sits, stands wide, then sways on a periodic
  schedule. `Train` sets the neutral posture, then kicks the legs in a loop.
- `Start()` blocks the main thread. Nothing in `Main` cancels the robot, calls
  `MaydayRobot.Stop()` or disposes the joint factory, so the behavior runs and
  the port stays open until the process is ended.
- `Train` builds and compiles a TensorFlow.NET model and prints its summary to
  the console.
- `FailFastFatalErrorHandler` writes the exception to the console before
  ending the process.

## Architectural Constraints

Starting the executable can open a port and move real actuators.

## Common Pitfalls

- [Physical runs](../knowledge/pitfalls/physical-runs.md); verify environment and map before execution.
- A numeric `mayday_startup_mode` such as `0`, or `Run,Train`, is accepted,
  and a lower-case `run` is not.

## Verification

- Run: `dotnet build Main\Main.csproj`. No hardware-free test covers
  `Program.cs` or `StartupMode`.
- The factory methods `Main` calls have tests in `Test`.
  `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.Robots"`
  calls `CreateWithSwayBehavior` but never runs the returned effect, so it
  checks little. It does not move the robot or open the serial port, but it is
  not hardware-free: building the effect already opens the FTDI adapter (serial
  number `FT2H2YMW`), asks it for 1 ms latency and closes it again, when the
  adapter is plugged in. `Test.Integration.Main.MaydayRobotTests`,
  despite its namespace, tests `MaydayRobotFactory` and `MaydayRobot`; its
  tests are `PhysicalRobotFact` and move the robot.
- Running `Main` itself moves the robot: run it only after the operator
  confirms the robot, port and operating conditions.

## Related ADRs

[Hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md), [Test tiers](../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md).

## Related Findings

None recorded.

## Related Root Causes

[Empty leg posture map](../knowledge/root-causes/empty-leg-posture-map.md) (why `Load` rejects an empty map).

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
