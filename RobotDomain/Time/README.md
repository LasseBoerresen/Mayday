# Shared timing

## Responsibility

Schedule periodic work and represent timed targets.

## Public APIs

`PeriodicScheduler`, `FatalErrorHandler`, `FailFastFatalErrorHandler`, `Timed<T>`, `HighResolutionWindowsTimerSetting`.

## Key Concepts

`TimeProvider`, cancellation, deadlines, timer resolution.

## Dependencies

`RobotDomain` and system timing APIs.

## Dependency Rules

Accept clocks from callers rather than hardcoding time inside planners.

## Invariants

`PeriodicScheduler` currently changes process priority and hands every exception from its action to the `FatalErrorHandler` it is constructed with, which has no default. Composition roots that drive a real robot pass `FailFastFatalErrorHandler`, which logs and calls `Environment.FailFast`; tests that use a mocked driver pass a recording handler. Physical tests keep `FailFastFatalErrorHandler`. If a handler returns, which a production handler never does, the loop ends instead of repeating the failure every period.

## Architectural Constraints

Scheduler changes can affect CPU usage and physical motion; never run a hardware loop as a casual test.

## Common Pitfalls

Do not replace periodic timing without reviewing deadlines, shutdown, and platform assumptions. See also [periodic loops in tests](../../knowledge/pitfalls/periodic-loops-in-tests.md).

## Related ADRs

[Hardware boundary](../../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

[Scheduler construction](../../knowledge/findings/scheduler-constructed-inside-domain-classes.md), [Wait ignores cancellation](../../knowledge/findings/periodic-scheduler-wait-ignores-cancellation.md)

## Related Root Causes

[Interpolation drift](../../knowledge/root-causes/interpolation-baseline-drift.md), [Test host crash](../../knowledge/root-causes/test-host-crash-from-scheduler-failfast.md), [Clean-runner failures](../../knowledge/root-causes/tests-failing-only-on-a-clean-runner.md)

## Related Lessons Learned

[Goal versus measured state](../../knowledge/lessons-learned/goal-versus-measured-state.md).

## Related Failed Attempts

[Measured-angle interpolation](../../knowledge/failed-attempts/measured-angle-interpolation.md).
