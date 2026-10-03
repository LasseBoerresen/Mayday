# PeriodicScheduler.Wait ignores cancellation

## Description

`PeriodicScheduler.Run` checks `ct.IsCancellationRequested` only between periods. Inside a period, `Wait` spins until the injected `TimeProvider` reports that the duration has elapsed, and it never looks at the token. `RunAsync` passes the token to `Task.Run`, which only prevents the task from starting.

With a real clock this delays shutdown by at most one period. With a `FakeTimeProvider`, which never advances by itself, a cancelled loop stays in `Wait` forever and spins a core until the process exits.

## Why It Matters

A test that cancels a scheduler loop and nothing else leaves the loop running. The cleanup has to cancel and then advance the fake clock past the current period, which `PeriodicSchedulerTests` now does in `Dispose`. Shutdown latency of up to one period also applies to the real control loops. Any change here affects cancellation and shutdown, which `RobotDomain/CLAUDE.md` lists as needing timing and safety reasoning.

## Evidence

`RobotDomain/Time/PeriodicScheduler.cs`: `Run` and `Wait`. The leaked loops were visible while diagnosing the [test host crash](../root-causes/test-host-crash-from-scheduler-failfast.md) and the [clean-runner failures](../root-causes/tests-failing-only-on-a-clean-runner.md). I did not measure the real shutdown latency.

## Recommendation

No change is planned. If the scheduler is touched, for example for the [fatal-error handler](scheduler-constructed-inside-domain-classes.md), consider checking the token inside `Wait`, and give the change its own commit with timing reasoning. Until then, tests that start a loop must advance the fake clock when they stop it.

## Related Components

[RobotDomain/Time](../../RobotDomain/Time/README.md), [Test](../../Test/README.md)

## Related Decisions

[ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
