# RobotDomain time unit tests

## Overview

Unit tests of `PeriodicScheduler` in `RobotDomain.Time`
([production README](../../../../RobotDomain/Time/README.md)). They run the
scheduler's real loop on a thread-pool thread, mostly under a
`FakeTimeProvider`, and check how often it calls the action and what happens
when the action throws.

## Interface

### `PeriodicSchedulerTests`

Checks the scheduler's cadence and its handling of a failing action.
File: `PeriodicSchedulerTests.cs`.

| Test | What it checks |
|---|---|
| `ShouldCallOnceActionIfTimeHasNotAdvancedEnough` | After the first call, advancing the fake clock by half a period (1 s period) causes no second call. |
| `ShouldCallActionTwiceAfterOneDurationIsPassed` | Advancing the fake clock by one period plus 1 ms causes exactly one more call. |
| `ShouldHandOverAnExceptionFromTheActionToTheFatalErrorHandler` | The exception the action throws reaches the `FatalErrorHandler`, once and as the same instance. |
| `ShouldStopTheLoopAfterTheFatalErrorHandlerReturns` | When the handler returns, the task from `RunAsync` completes by itself after one call, without cancelling or advancing the clock. |
| `GivenTheRealClockAnd20MsPeriod_When100PeriodsRun_ThenMeanIsWithin1PercentAnd99PercentAreWithin1Ms` | On `TimeProvider.System` with a 20 ms period (the strictest the robots use), the mean of 100 periods is within 1% (0.2 ms) and at least 99 of them are within 1 ms. One stalled period is tolerated: Windows occasionally holds the thread back by a few milliseconds. |

The class implements `IDisposable`: `Dispose` cancels the shared token and
advances the fake clock by two periods, so the loop sees the cancellation and
ends.

## Invariants

- A fake clock does not step the scheduler: the loop checks cancellation only
  between periods, so a test must advance the clock past the current period
  after cancelling (`Dispose`), see the
  [finding](../../../../knowledge/findings/periodic-scheduler-not-steppable-under-fake-time.md).
- Waits are bounded and condition-based: `WaitUntil` spins until the condition
  holds or `MaxWaitForScheduler` (10 s) passes, because the loop may start late
  on a busy machine. The only fixed sleep, 1 ms in
  `ShouldCallOnceActionIfTimeHasNotAdvancedEnough`, cannot make a correct
  scheduler fail.
- Counters are shared with the loop thread through `Interlocked.Increment` and
  `Volatile.Read`.
- Every scheduler gets a `RecordingFatalErrorHandler`, so an action failure is
  an assertion, not a crash of the test host
  ([pitfall](../../../../knowledge/pitfalls/periodic-loops-in-tests.md)).
- Only the 20 ms cadence test uses the real clock, because the period itself
  is what it measures. It records one `Stopwatch` timestamp per call into a
  preallocated array and calls the action once before the loop starts, so
  neither allocation nor compilation inside the action shifts a period. It
  takes about 2 s.

## Side Effects

- Creating the first `PeriodicScheduler` in the test process runs its static
  constructor: it sets the process priority class to `High` and, through
  `HighResolutionWindowsTimerSetting`, calls `timeBeginPeriod(1)` in
  `winmm.dll` until the process exits. Both last for the whole test run.
- Each test starts a loop on a thread-pool thread that busy-waits
  (`Thread.SpinWait`) while a period runs, using a CPU core until it ends.
- The scheduler writes to the console when an action overruns its period.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.RobotDomain.Time"`
