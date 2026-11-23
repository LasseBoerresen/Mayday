# Parallel tests stalled the real-clock cadence test

## Symptoms

After commit `98d1b95` took it out of quarantine, the scheduler's real-clock cadence test (`GivenTheRealClockAnd20MsPeriod_When100PeriodsRun_ThenMeanIsWithin1PercentAnd99PercentAreWithin1Ms`) failed the local gate's first run. Then:

- It failed 4 of 25 full runs of the blocking filter (`Quarantine!=true&Tier!=Physical&Tier!=Manual`).
- Every failure said `but found 98`: two of 100 periods missed 20 ms by more than 1 ms, where the test tolerates one.
- It passed when run alone.

## Investigation

- **Instrumented full runs:** a temporary edit wrote every period that missed by more than 1 ms to a file. In 25 full runs:
  - 17 runs had at least one miss and 10 had two or more.
  - The misses were 21.7 to 30.3 ms, which are stalls of 1.7 to 10.3 ms, never early periods except one 17.9 ms period right after a long one.
  - Nearly all fell in the first 25 periods, the first half-second, while xUnit was still running the other test classes in parallel.
  - One of those runs would have failed on the mean instead, which the test checks before the count: its mean was 20.206 ms, past the 1% bound of 20.2 ms. None of the 4 uninstrumented failures was of this kind.
- **Alone:** 7 runs of the test alone had no miss.
- **The estimate in `98d1b95`:** its message estimated stalls at about 1 period in 1,500 to 4,500, up to 6.3 ms "under test load". Inside the parallel suite the rate was about 1 in 100 periods and the longest stall 10 ms. That message does not say how the estimate was measured.

## Root Cause

xUnit runs test collections in parallel by default, and every test class is its own collection. The cadence test shared `PeriodicSchedulerTests` with the fake-clock tests and ran beside every other test class. Their CPU load in the first half-second of the run stalled the scheduler's busy-waiting thread often enough that two stalls in 100 periods were common. The scheduler itself is not at fault: without the load, it holds the cadence.

## Resolution

PR #71, `fix: run the scheduler cadence test without parallel tests`:

- moved the test, unchanged, into `PeriodicSchedulerCadenceTests`;
- put that class in a new `Test.Utilities.RealClockCollection`, defined with `DisableParallelization = true`, which xUnit runs alone after every parallel collection.

After the change, 0 of 25 full runs failed, and the physical pre-push gate passed. The thresholds were kept: loosening the 99-of-100 bound would hide the jitter the test exists to catch.

## Prevention

- Put any test that asserts timing on the real clock in `RealClockCollection` ([Test/Utilities](../../Test/Utilities/README.md#realclockcollection), [pitfall](../pitfalls/periodic-loops-in-tests.md)).
- Before a timing test leaves quarantine, measure its failure rate over repeated full-suite runs, the way the gate runs it, not alone. A test that is stable alone can still fail in the suite.
- When a timing test fails, record which periods missed and by how much. The positions of the misses (early in the run) pointed to the parallel tests.

## Related Components

[RobotDomain/Time](../../RobotDomain/Time/README.md), [Test](../../Test/README.md), [Test/Unit/RobotDomain/Time](../../Test/Unit/RobotDomain/Time/README.md), [Test/Utilities](../../Test/Utilities/README.md)

## Related Tasks

[MAY-83: Need: PeriodicScheduler keeps a 20 ms cadence on the real clock](https://linear.app/mayday-robot/issue/MAY-83/need-periodicscheduler-keeps-a-20-ms-cadence-on-the-real-clock), where the test's thresholds were agreed.

## Related Decisions

[ADR 0004: Quarantine failing tests with a trait](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md), [ADR 0006: Run quality gates locally](../../docs/adr/0006-run-quality-gates-locally.md)
