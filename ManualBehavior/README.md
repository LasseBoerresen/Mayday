# ManualBehavior

## Overview

Mayday's behavior controllers: they decide what the robot does next and hand
it to a `MaydayMotionPlanner`. Despite the project's name, only one of them is
manual; the other two run on their own once started. Renaming the project to
match is planned.

## Responsibility

Decide Mayday's behavior, from terminal commands or automatically, and turn it
into postures, tip movements or motion goals for the motion planner.

## Public APIs

`SwayBehaviorController`, `BabyLegsBehaviorController`,
`TerminalPostureBehaviorController`, `PostureCommand`.

## Interface

All types are in namespace `ManualBehavior`. Each controller implements
`RobotDomain.Behavior.BehaviorController`, whose single method `Start()` runs
the behavior on the calling thread and returns `Unit` when it ends. The
`Robots` factory `MaydayRobotFactory` constructs them; `MaydayRobot.Start()`
calls `Start()` and `MaydayRobot.Stop()` cancels the token they watch, which
also stops the joint driver (see Invariants).

| Controller | Kind | Driven by | Factory method |
|---|---|---|---|
| `SwayBehaviorController` | Automated | A periodic schedule | `CreateWithSwayBehavior` (`Main`'s `Run` mode) |
| `BabyLegsBehaviorController` | Automated | A loop of random kicks | `CreateWithBabyLegsBehaviorController` (`Main`'s `Train` mode) |
| `TerminalPostureBehaviorController` | Manual | Commands typed in a `Terminal` | `CreateWithTerminalPostureBehaviorController` |

### Automated behaviors

#### `SwayBehaviorController`

Wakes the robot, then sways its thorax around the standing position by
setting a new random lean goal every 2 seconds. File:
`SwayBehaviorController.cs`.

Constructor: `(MaydayMotionPlanner motionPlanner, CancellationToken ct,
TimeProvider timeProvider, FatalErrorHandler fatalErrorHandler)`.

`Start()`, in order:

1. Wakes up: `SetPosture(MaydayLegPosture.Sitting)`, then
   `SetPosture(MaydayLegPosture.StandingWide)`, each scheduled 1 s ahead and
   followed by a blocking 1 s `Thread.Sleep`.
2. Calls `motionPlanner.Start(ct)`, so the planner starts tracking its goal.
3. Runs a `PeriodicScheduler` (built with `timeProvider` and
   `fatalErrorHandler`) that sets `motionPlanner.Goal` once per 2 s period
   until `ct` is cancelled, then returns.

Each new goal, a `Timed<Motion>` due 0.2 s after it is issued:

| Step | What it does |
|---|---|
| Start from | The planner's current `Goal` target, or `Motion.StandingStill` when it has none. |
| Add sway | `Transform.Random(0.07 m, Angle.Zero)` added to `Lean`: up to ±0.07 m on each axis, no rotation. |
| Pull to center | Moves `Lean` halfway (`factor: 0.5`) towards `Motion.StandingStill.Lean`. |
| Report | Writes `Goal lean: <xyz>` to the console. |

Only `Lean` changes; `Twist` and `StanceRadius` keep the previous goal's
values.

#### `BabyLegsBehaviorController`

Sets the neutral posture, then kicks every leg tip by a small random amount
in a loop until cancelled. File: `BabyLegsBehaviorController.cs`.

Constructor: `(MaydayMotionPlanner motionPlanner, CancellationTokenSource
cancelTokenSource, TimeProvider timeProvider)`. It only reads the source's
`Token`; it never cancels it.

`Start()`, in order:

1. `SetPosture(MaydayLegPosture.Neutral)` scheduled 1 s ahead, then a
   blocking 1 s `Thread.Sleep`.
2. Until the token is cancelled: calls `motionPlanner.MoveTipPositions` with a
   random delta of up to ±0.02 m on each axis for each of the six tips, due
   immediately (0 s ahead), with no pause between kicks.

It does not call `motionPlanner.Start`.

### Manual behavior

#### `TerminalPostureBehaviorController`

Moves the robot into the posture named by each command typed in a terminal.
Derives from `RobotDomain.Behavior.TerminalBehaviorController<PostureCommand>`,
which owns the read-command loop. File: `TerminalPostureBehaviorController.cs`.

Constructor: `(MaydayMotionPlanner motionPlanner, Terminal terminal,
CancellationToken cancelToken, TimeProvider timeProvider)`.

`Start()`, inherited, in order:

1. Writes a blank line and `Waking up...` to the terminal and wakes up: `Sitting`,
   `SittingTall`, `Sitting`.
2. Writes the command list (`<number>: <name>`) to the terminal.
3. Until the token is cancelled or `Terminal.ReadLine` returns `null`: prompts
   `Next command: `, executes the command, then writes the tip positions and
   orientations (`GetPositionsOf`/`GetOrientationsOf(LinkName.Tip)`) to the
   console.

Each posture is set with `SetPosture`, scheduled 1 s ahead and followed by a
blocking 1 s `Thread.Sleep`. It does not call `motionPlanner.Start`.

Input is parsed with `Enum.TryParse<PostureCommand>(..., ignoreCase: true)`:
names in any case and numbers are accepted. Anything that does not parse
writes `Invalid command: '<input>'` and prompts again. A number with no
`PostureCommand` member, such as `99`, parses and moves nothing, but the tip
positions are still printed. Comma-separated names combine bitwise:
`Wake,Sleep` is 3, which is `Stand`.

#### `PostureCommand`

The commands the terminal controller accepts. Enum, file `PostureCommand.cs`.

| Command | Value | Effect |
|---|---|---|
| `Wake` | 1 | `Waking up...`, then `Sitting`, `SittingTall`, `Sitting`. |
| `Sleep` | 2 | `Going to sleep...`, then `Sitting`. |
| `Stand` | 3 | `MaydayLegPosture.Standing` |
| `StandWide` | 4 | `MaydayLegPosture.StandingWide` |
| `StandHigh` | 5 | `MaydayLegPosture.StandingHigh` |
| `Sit` | 6 | `MaydayLegPosture.Sitting` |
| `SitTall` | 7 | `MaydayLegPosture.SittingTall` |
| `Neutral` | 8 | `MaydayLegPosture.Neutral` |
| `NeutralWithBackTwist` | 9 | `MaydayLegPosture.NeutralWithBackTwist` |
| `Straight` | 10 | `MaydayLegPosture.Straight` |
| `StraightWithBackTwist` | 11 | `MaydayLegPosture.StraightWithBackTwist` |
| `NeutralWithStraightFemur` | 12 | `MaydayLegPosture.NeutralWithStraightFemur` |

## Key Concepts

Behavior controller, automated vs. terminal-driven behavior, timed posture
sequencing, sway goal (random lean pulled towards standing still).

## Dependencies

`MaydayDomain` (`MaydayMotionPlanner`, `MaydayLegPosture`, `Motion`,
`MaydayStructureSet`), `RobotDomain` (`BehaviorController`,
`TerminalBehaviorController`, `PeriodicScheduler`, `FatalErrorHandler`,
`Timed`, `Transform`, `Xyz`, `LinkName`), `Generic` (`Terminal`, through
`RobotDomain`'s reference), `UnitsNet` and the `LanguageExt.Sys` package.
`ManualBehavior.csproj` also references `MaydayDataAccess`, which no code here
uses.

## Dependency Rules

Behavior selects goals; motion planning and device communication belong to their own layers.

This is the intended rule, not yet the current state. Only
`SwayBehaviorController` sets goals (`motionPlanner.Goal`), and only after its
wake-up. Its wake-up, `TerminalPostureBehaviorController` and
`BabyLegsBehaviorController` still command the planner directly with
`SetPosture` and `MoveTipPositions`.

## Invariants

- Cancelling the token (`MaydayRobot.Stop()`) asks the behavior to shut down;
  it is not an emergency stop. The behavior's loop finishes its current step
  first, and that wait is intended.
- When composed by `MaydayRobotFactory`, the same `CancellationTokenSource`
  also drives `PeriodicallyBatchedJointDriver`, the only path to the servos.
  `Stop()` therefore ends the goal writes within one 20 ms period: the robot
  holds its last commanded position, and postures the behavior still sets
  never reach the servos.
- Every `Start()` blocks the calling thread until its behavior ends. The token
  is checked only between steps: a kick, a sway period (2 s) or a terminal
  command. The wake-up sequence runs before any check, so after a stop during
  wake-up `Start()` still blocks until the sequence's sleeps end (Sway 2 s,
  terminal 3 s, baby legs 1 s), without moving the robot.
- The terminal controller waits in `Terminal.ReadLine()` for each command,
  and re-prompts after invalid input without checking the token: a cancel ends
  `Start()` only after the next line, or when `ReadLine` returns `null`.
- Posture sequences are timed: each posture is scheduled 1 s ahead and the
  next one is not set until that second has passed.
- `SwayBehaviorController` sets its wake-up postures before it starts the
  planner's tracking, and after that changes only `motionPlanner.Goal`. Planned
  (a `TODO` in `WakeUp`): set a movement goal instead of postures.
- Sway goals stay near standing still: each one moves the lean halfway back to
  `Motion.StandingStill.Lean` after adding at most ±0.07 m per axis.
- `BabyLegsBehaviorController` needs a planner that supports
  `MoveTipPositions`: `TrackingMaydayMotionPlanner` throws
  `NotSupportedException`, so the factory pairs it with
  `StepByStepLearningMaydayMotionPlanner`.
- An exception in a sway period goes to the injected `FatalErrorHandler`; if
  the handler returns, the sway ends. Exceptions in the other controllers
  propagate out of `Start()`.

## Side Effects

- All three move the real robot through the motion planner when composed by
  `MaydayRobotFactory`, which opens the Dynamixel port.
- `SwayBehaviorController` starts the planner's tracking loop, which runs on a
  thread-pool thread until `ct` is cancelled. Its `PeriodicScheduler` raises
  the process priority to `High` and requests the high-resolution Windows
  timer (on first use of the type). The scheduler busy-waits
  (`Thread.SpinWait`) through each period, so the calling thread keeps a CPU
  core busy for as long as the sway runs, as does the planner's tracking loop.
  It writes each goal lean to the console, and the scheduler writes
  `Action exceeded time slot ...` when a period overruns.
- `BabyLegsBehaviorController` kicks without pausing; any rate limit comes
  from the planner (`StepByStepLearningMaydayMotionPlanner` sleeps 1 s after
  each movement).
- `TerminalPostureBehaviorController` reads from and writes to its `Terminal`,
  and writes tip positions and orientations to the console, not the terminal.
- All three block their thread with `Thread.Sleep` during posture steps.

## Architectural Constraints

Avoid constructing concrete actuator hardware inside controllers; `Robots` composes dependencies.

## Common Pitfalls

- [Physical runs](../knowledge/pitfalls/physical-runs.md); do not assume a behavior test is hardware-free.
- The project name says "manual", but `SwayBehaviorController` and
  `BabyLegsBehaviorController` run without input once started. A rename is
  planned.

## Verification

- Run: `dotnet build ManualBehavior\ManualBehavior.csproj`. No
  hardware-free test covers these controllers.
- `Test.Integration.Main.MaydayRobotTests` starts the terminal and baby-legs
  robots on a background task, calls `Stop()` after 0.5 s and returns without
  waiting for `Start()`. The robot stops being driven at `Stop()`, but
  `Start()` keeps running on its thread until the wake-up's sleeps end. Their
  only check is that nothing throws on the test thread; the terminal robot
  ends after wake-up anyway, because `TestTerminal([])` has no input. Both tests are `PhysicalRobotFact` and move the robot: run them only after the operator confirms the robot, port and
  operating conditions. No test starts `SwayBehaviorController`.

## Related ADRs

[Stable abstractions](../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

[PeriodicScheduler constructed inside domain classes](../knowledge/findings/scheduler-constructed-inside-domain-classes.md) (`SwayBehaviorController` builds its own scheduler).

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
