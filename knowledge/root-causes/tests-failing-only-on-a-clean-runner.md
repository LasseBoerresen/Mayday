# Tests that passed locally failed on a clean CI runner

## Symptoms

The first run of the `test` job on a clean GitHub-hosted Windows runner (pull request #28) failed 8 tests, while the same set passed locally (209 passed, 9 skipped, 0 failed):

- 6 physical tests failed during discovery with `FileNotFoundException: The configuration file 'secrets.json' was not found and is not optional`, expected under `Test\bin\Release\net10.0`.
- 2 `PeriodicSchedulerTests` failed with "Expected counter to be 1, but found 0" and "Expected counter to be 2, but found 0".

## Investigation

- **Secrets:** `TestConfiguration.Create()` called `AddUserSecrets<TestConfiguration>(optional: false)`, and `PhysicalRobotFact` calls it from its constructor, so the exception surfaced at discovery. Setting `MAYDAY_ROBOT_IS_CONNECTED=False` on the runner did not help, because the configuration build throws before the variable is read. No physical test ran, so nothing touched hardware.
- **Scheduler tests:** both started the loop with `Task.Run`, slept 1 ms, and then asserted the action had run. A busy runner had not started the thread-pool thread in that time.
- **Reproduction:** 56 busy loops on a 28-core machine did not reproduce it (0 of 15 runs failed). Confining the test process to one core, with busy loops pinned to that core, failed 10 of 10 runs with the CI messages.
- The tests also never stopped their loops, and `PeriodicScheduler` cannot be stopped by cancelling alone ([finding](../findings/periodic-scheduler-wait-ignores-cancellation.md)).

## Root Cause

1. A test-configuration source was required that only exists on a developer machine, so the physical tests could not even be discovered elsewhere. The class already treated a missing value as "not connected", so the requirement contradicted its own intent.
2. Two tests assumed a background thread would run within a fixed 1 ms, a timing assumption that holds on a fast, idle machine.

## Resolution

- `f6b788e`: user secrets are optional. A machine without them has no robot connected, and physical tests still need an explicit `True`.
- `c756cba`: the scheduler tests wait for the action (bounded at 10 s) and stop their loop in `Dispose`. Under the one-busy-core condition they passed 20 of 20 runs.
- After both, the `test` job passed on a clean runner with 209 passed, 9 skipped, 0 failed, and was added to the gate.

## Prevention

- Run a new CI job non-blocking before gating it, as [ADR 0003](../../docs/adr/0003-gate-pull-requests-with-one-aggregate-check.md) does. This is how both problems surfaced without blocking a merge.
- Reproduce a timing failure by constraining the machine, not by adding load to a many-core one: pin the test process to one core and put the load on that core.
- Do not make tests depend on files or secrets that exist only on a developer machine, and do not assume a background thread runs within a fixed, short sleep. Wait for the event with a generous bound.

## Related Components

[Test](../../Test/README.md), [RobotDomain/Time](../../RobotDomain/Time/README.md)

## Related Tasks

[PR gating and test baseline](../task-summaries/pr-gating-and-test-baseline.md)

## Related Decisions

[ADR 0003](../../docs/adr/0003-gate-pull-requests-with-one-aggregate-check.md), [ADR 0005](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md)
