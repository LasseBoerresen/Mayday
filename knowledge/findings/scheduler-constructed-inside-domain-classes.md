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

The owner chose to pass a fatal-error handler through the constructors from the composition roots, in line with the repository rule to pass dependencies explicitly. The handler would default to today's `FailFast` behavior in production and let tests record the error. **This is planned and not implemented.** It touches control-loop code, so its commit needs timing and safety reasoning.

## Related Components

[RobotDomain/Time](../../RobotDomain/Time/README.md), [RobotDomain/Motion](../../RobotDomain/Motion/README.md), [MaydayDomain](../../MaydayDomain/README.md), [ManualBehavior](../../ManualBehavior/README.md), [EllieMain](../../Ellie/EllieMain/README.md)

## Related Decisions

[ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
