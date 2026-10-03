# Quarantine failing tests with a trait

## Status

Accepted

## Context

When this was decided, 46 of 212 tests in `Test` failed and one crashed the test host, so a test run could not gate a merge. A red run said nothing about a new change. The failures had five separate causes ([empty map](../../knowledge/root-causes/empty-leg-posture-map.md), `Q` validation, a Moq proxy mismatch, a stale test, an assertion mismatch).

## Decision

`[Quarantine("reason")]` (`Test/Utilities/QuarantineAttribute.cs`) marks a test method or class. It produces the traits `Quarantine=true` and `QuarantineReason`. `dotnet test --filter "Quarantine!=true"` is the blocking run, and `--filter "Quarantine=true"` lists what is still broken. Quarantine records a failure with its cause; it never changes what a test expects. A test leaves quarantine when the test or the code is fixed.

## Alternatives Considered

- `Skip = "reason"`: the test stops running, so nobody notices when it starts passing.
- An external list of known failures: it drifts from the code.
- Leaving the tests red: the run cannot gate anything.

## Why

Quarantined tests still run, in their own non-blocking step (planned), so recovery shows up. Releasing tests as causes were fixed is observable: after the empty-map fix 36 of the 46 left quarantine, and two of the released tests revealed real assertion mismatches that the exception had hidden. They were quarantined again with their true reason.

## Consequences

- Benefits: a trustworthy blocking set (198 passed, 0 failed, 9 skipped after the echo fix); every known failure carries a reason in code.
- Costs: the attribute can be abused to hide a regression. Tagging a test is a review point, and the planned gate-file protection should cover it.
- Limitations: eight tests are quarantined today. The separate non-blocking CI step does not exist yet. `EllieMainTests` has two pre-existing failures with no quarantine.

## Related Components

[Test](../../Test/README.md)

## Related ADRs

[0003](0003-gate-pull-requests-with-one-aggregate-check.md), [0005](0005-test-tiers-for-physical-and-simulated-runs.md)

## Related Findings

None recorded.

## Related Root Causes

[Empty leg posture map](../../knowledge/root-causes/empty-leg-posture-map.md), [Test host crash](../../knowledge/root-causes/test-host-crash-from-scheduler-failfast.md)
