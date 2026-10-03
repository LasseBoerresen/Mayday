# Pitfall

## Description

Constructing a `PeriodicallyBatchedJointDriver` in a test without configuring the mocked bus or disposing the driver.

## Why It Is Dangerous

The constructor starts a loop that runs every millisecond on a background thread. Any exception there calls `Environment.FailFast` and ends the whole test process, long after the test that started the loop has finished. Moq defaults (`null` for an unconfigured dictionary, `0` for a number) are values the real bus never returns. Both crash sources in the [test host crash](../root-causes/test-host-crash-from-scheduler-failfast.md) came from this, and the failing test the runner names is often a bystander.

## Warning Signs

"Test host process crashed", a stack through `PeriodicScheduler.CallActionWithErrorLogging`, a crash that depends on which other tests run, or a test class that passes alone and aborts the full run. A test class that builds a driver but does not implement `IDisposable`.

## Preferred Approach

Configure every bus read the loop and `Initialize` use (`Read` for several ids and for one id), report a plausible position such as `StepAngle.StepCenter`, and dispose the driver when the test ends. Pass `RecordingFatalErrorHandler` wherever a scheduler is built, so a loop error is an assertion failure, not a lost run; physical tests keep `FailFastFatalErrorHandler`. Prefer `EchoJointDriver` or a fake `TimeProvider` where they fit. After a change, run the whole `Test` project and check that it completes, not only the class you changed.

## Related Components

[RobotDomain/Motion](../../RobotDomain/Motion/README.md), [RobotDomain/Time](../../RobotDomain/Time/README.md), [Test](../../Test/README.md)
