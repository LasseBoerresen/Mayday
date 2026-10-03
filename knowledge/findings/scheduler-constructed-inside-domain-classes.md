# PeriodicScheduler is constructed inside four production classes

## Description

`PeriodicScheduler` is created with `new PeriodicScheduler(timeProvider)` inside four classes, not at a composition root:

- `RobotDomain/Motion/PeriodicallyBatchedJointDriver.cs`
- `MaydayDomain/MotionPlanning/TrackingMaydayMotionPlanner.cs`
- `ManualBehavior/SwayBehaviorController.cs`
- `Ellie/EllieMain/MotionPlanning/ArticulatedSteeringEllieMotionPlanner.cs`

Its `CallActionWithErrorLogging` ends the process with `Environment.FailFast` on any exception from the periodic action, and the code carries a TODO about an injectable handler.

## Why It Matters

A test cannot choose what happens when a periodic action throws, because the scheduler is built out of its reach. Any test mistake in a loop then ends the whole test process ([root cause](../root-causes/test-host-crash-from-scheduler-failfast.md)). It also leaves no place to put a real "stop the robot safely" reaction, since the fail-fast policy is hard-coded in the control loop. `RobotDomain/CLAUDE.md` says to keep the fail-fast and the high process priority unless the change is about them.

## Evidence

The four `new PeriodicScheduler(...)` call sites above; `RobotDomain/Time/PeriodicScheduler.cs`, `CallActionWithErrorLogging`; the crash investigation in the linked root cause.

## Recommendation

The owner chose to pass a fatal-error handler through the constructors from the composition roots, in line with the repository rule to pass dependencies explicitly. **This was implemented in `cc9ab54` and `7eced7a`.** The handler has no default: `Main` and `EllieMain` pass `FailFastFatalErrorHandler`, which keeps today's production behavior, tests with a mocked driver pass a recording handler, and physical tests keep fail-fast. A handler that returns ends the loop. The change touched control-loop code and left cadence, priority, the wait loop and cancellation alone.

## Related Components

[RobotDomain/Time](../../RobotDomain/Time/README.md), [RobotDomain/Motion](../../RobotDomain/Motion/README.md), [MaydayDomain](../../MaydayDomain/README.md), [ManualBehavior](../../ManualBehavior/README.md), [EllieMain](../../Ellie/EllieMain/README.md)

## Related Decisions

[ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
