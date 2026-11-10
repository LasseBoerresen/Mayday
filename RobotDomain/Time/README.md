# Shared timing

## Responsibility

Model anything time related in robotics, such as periodic scheduling, timed targets, clocks and timer resolution. Today it holds the periodic scheduler, timed targets and the Windows timer setting, plus the fatal error handlers the scheduler uses (see Known Issues).

## Interface

- **Scheduling:** `PeriodicScheduler(TimeProvider timeProvider, FatalErrorHandler fatalErrorHandler)` (`PeriodicScheduler.cs`):
  - `Run(Action action, Duration duration, CancellationToken ct)`: blocking; calls `action` once per `duration` until `ct` is cancelled.
  - `RunAsync(…)`: the same loop on a thread-pool thread (`Task.Run(…, ct)`); the task completes when the loop ends.
- **Fatal errors:** `FatalErrorHandler`, an interface with `Handle(Exception)`, called on the scheduler's thread for an exception that escaped the periodic action (`FatalErrorHandler.cs`). `FailFastFatalErrorHandler`, a sealed implementation for composition roots that drive a real robot, writes the message and stack trace to the console, then calls `Environment.FailFast` (`FailFastFatalErrorHandler.cs`).
- **Timer resolution:** `HighResolutionWindowsTimerSetting.Instance`, a process-wide singleton holding a 1 ms Windows timer resolution; `VerifyTimerResolution()` prints the current resolution (`HighResolutionWindowsTimerSetting.cs`).
- **Timed targets:** `Timed<T>(DateTimeOffset ArrivalTime, DateTimeOffset IssueTime, T Target)`, an immutable record (`Timed.cs`).
  - `Passed(T)`: both times `DateTimeOffset.MinValue`, so the target is already reached.
  - `Map(Func<T, TNew>)`, `ExtendWith(TimeSpan)`: copies with the target mapped, or the arrival time moved later.
  - `StepFactor(DateTimeOffset currentTime)`: progress from issue to arrival, clamped to [0, 1]; 1.0 when the span is zero or negative.
  - `TimedExtensions`: `Sequence()` splits a `Timed<IEnumerable<T>>` into one `Timed<T>` per element with the same times; `TimeProvider.ScheduleIn(target, arrivalTime)` returns a `Timed` issued now and arriving `arrivalTime` later.

## Key Concepts

- **Fixed rate:** each period starts `duration` after the previous one started. An overrun is logged and the next period starts at once; missed periods are never caught up.
- **Injected clock:** every time reading goes through the `TimeProvider` passed in, so tests can control time.
- **Fatal by default:** an exception escaping a periodic action is fatal. The `FatalErrorHandler` decides what that means; on a real robot it ends the process.
- **Timed target:** a target paired with when it was issued and when it should be reached. `StepFactor` says how far along that span a moment is; a `Passed` target is already reached.

## Dependencies

Part of `RobotDomain`. Uses:
- `UnitsNet` for `Duration`;
- `Generic` for `ClampToPositive`;
- `System.Diagnostics.Process` for the process priority;
- the Windows `winmm.dll` and `ntdll.dll` through P/Invoke, for the timer resolution.

## Dependency Rules

Accept clocks from callers rather than hardcoding time inside planners.

## Invariants

- **Errors:** every exception from the action goes to the `FatalErrorHandler` the scheduler was constructed with; there is no default. If a handler returns, the loop ends instead of repeating the failure every period. Exceptions thrown by `Handle` itself are not caught and propagate out of `Run`/`RunAsync`. `FailFastFatalErrorHandler.Handle` never returns.
- **Cancellation:** checked once per period, before the action; neither the action nor the wait observes `ct`. If `ct` is already cancelled, `RunAsync` returns a cancelled task without running the loop.
- **Cadence:** each period starts at `timeProvider.GetUtcNow()` before the action, and the wait busy-spins (`Thread.SpinWait`) until `duration` has elapsed. A fake `TimeProvider` that never advances makes the wait spin forever.
- **Validation:** `duration` is not validated.
- **Platform:** the first use of `PeriodicScheduler` in a process forces `HighResolutionWindowsTimerSetting.Instance` and sets the process priority to `High` (static constructor). There is no platform check: the `winmm.dll`/`ntdll.dll` calls assume Windows.

## Known Issues

- `FatalErrorHandler` and `FailFastFatalErrorHandler` are error-handling policy, not time: [MAY-106: Need: Fatal error handling lives outside RobotDomain.Time](https://linear.app/mayday-robot/issue/MAY-106).
- On a platform without the Windows timer API, the first use fails far from the cause: [MAY-93: Need: PeriodicScheduler fails with a clear error on a platform without the Windows timer API](https://linear.app/mayday-robot/issue/MAY-93).
- `Environment.FailFast` skips `ProcessExit`, so the fail-fast path may leave the timer resolution raised: [MAY-94: Need: The Windows timer resolution is released when a fail-fast ends the process](https://linear.app/mayday-robot/issue/MAY-94).
- The cadence is not verified on the real clock: [MAY-83: Need: PeriodicScheduler keeps a 1 ms period within 1% on the real clock](https://linear.app/mayday-robot/issue/MAY-83).
- The wait always busy-spins one core, though its comment says it yields; the sleep is commented out and its fields are unused: [MAY-107: Need: PeriodicScheduler's wait has no dead or misleading sleep code](https://linear.app/mayday-robot/issue/MAY-107).
- The wait ignores cancellation, so shutdown can take up to one period ([finding](../../knowledge/findings/periodic-scheduler-wait-ignores-cancellation.md)), and a test cannot step the scheduler one period at a time under a fake clock ([finding](../../knowledge/findings/periodic-scheduler-not-steppable-under-fake-time.md)). No change is planned for either.
- `HighResolutionWindowsTimerSetting` is a public implementation detail, a candidate for an `Internal` namespace, and `VerifyTimerResolution` prints the resolution instead of returning it.

## Side Effects

- Process priority set to `ProcessPriorityClass.High` (`PeriodicScheduler` static constructor).
- Windows timer resolution: `timeBeginPeriod(1)` when `HighResolutionWindowsTimerSetting.Instance` is built, and `timeEndPeriod(1)` from an `AppDomain.ProcessExit` handler. `VerifyTimerResolution()` calls `NtQueryTimerResolution`.
- `Environment.FailFast` from `FailFastFatalErrorHandler.Handle`.
- Console output: the fail-fast error, overrun of a period (`Action exceeded time slot …`), and the current timer resolution.
- `RunAsync` holds a thread-pool thread busy-spinning for the life of the loop; `Run` does the same on the caller's thread.

## Verification

`dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.RobotDomain.Time"`

## Architectural Constraints

Scheduler changes can affect CPU usage and physical motion; never run a hardware loop as a casual test.

## Common Pitfalls

Do not replace periodic timing without reviewing deadlines, shutdown, and platform assumptions. See also [periodic loops in tests](../../knowledge/pitfalls/periodic-loops-in-tests.md).

## Related ADRs

[Hardware boundary](../../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

[Scheduler construction](../../knowledge/findings/scheduler-constructed-inside-domain-classes.md), [Wait ignores cancellation](../../knowledge/findings/periodic-scheduler-wait-ignores-cancellation.md), [Not steppable under a fake clock](../../knowledge/findings/periodic-scheduler-not-steppable-under-fake-time.md)

## Related Root Causes

[Test host crash](../../knowledge/root-causes/test-host-crash-from-scheduler-failfast.md), [Clean-runner failures](../../knowledge/root-causes/tests-failing-only-on-a-clean-runner.md)

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
