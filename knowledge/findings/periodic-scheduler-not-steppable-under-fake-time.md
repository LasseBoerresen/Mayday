# PeriodicScheduler cannot be stepped under a fake clock

## Description

`PeriodicScheduler.Wait` busy-spins on `timeProvider.GetUtcNow()` until the period has elapsed. With a `FakeTimeProvider`, which never advances by itself, the loop stays in `Wait` until another thread advances the clock, so a test cannot run "one period" and then assert. `PeriodicallyBatchedJointDriverTests` therefore waits on the real clock (`SpinWait.SpinUntil`, 1 s) for the driver's batched goal write.

A steppable scheduler, one whose test can advance time and observe exactly one action per period, was wished for in an old test TODO ("control time in the PeriodicScheduler to be able to properly test"). That TODO was removed with the quarantined test it sat in; this record keeps the idea.

## Why It Matters

Tests of the periodic joint driver can only assert "eventually written", not "written within one period" or "written exactly once per period". A latency or cadence contract (the driver's default period is 20 ms) is not covered by a deterministic test. Making the scheduler steppable changes cadence, cancellation and shutdown behavior, which `RobotDomain/CLAUDE.md` lists as needing timing and safety reasoning.

## Evidence

`RobotDomain/Time/PeriodicScheduler.cs`: `Run` and `Wait`. `Test/Unit/Dynamixel/PeriodicallyBatchedJointDriverTests.cs`: `GivenInitializedJoint_WhenSetGoalAngle_ThenBatchWriteContainsStepsForThatJoint`. `PeriodicSchedulerTests` already works around the same property by advancing the fake clock in `Dispose`, see [Wait ignores cancellation](periodic-scheduler-wait-ignores-cancellation.md).

## Recommendation

No change is planned. If the scheduler is touched (for example to check the token inside `Wait`), consider an injectable wait step that a test can drive, in its own commit with timing reasoning. Until then, tests of loops wait on the real clock with a generous timeout and assert "eventually", never a tight wall-clock bound.

## Related Components

[RobotDomain/Time](../../RobotDomain/Time/README.md), [Test](../../Test/README.md)

## Related Decisions

None recorded.
