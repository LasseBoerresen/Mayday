# Test

## Responsibility

Mayday and shared-layer xUnit unit/integration tests and test utilities.

## Public APIs

`Unit`, `Integration`, and `Utilities` include test mothers, fake joints, terminal, and physical-test attributes.

## Key Concepts

Hardware-independent tests by default; physical tests explicitly marked `PhysicalRobotFact`/`PhysicalRobotTheory`.

A test known to fail is marked `[Quarantine("reason")]` (`Utilities/QuarantineAttribute.cs`), on the method or the class. It gets the trait `Quarantine=true`, so `dotnet test --filter "Quarantine!=true"` is the blocking run and `--filter "Quarantine=true"` lists what is still broken. Quarantine records a failure with its cause; it never changes what a test expects. Fix the test or the code, then remove the attribute.

For multi-value or domain-specific parameterized cases, prefer a test-specific `TheoryData<TestInput>` with named `TestInput` members over positional `[InlineData]`. See [ThoraxTests](Unit/Components/ThoraxTests.cs) and the [test guidance](CLAUDE.md).

## Dependencies

References Mayday runtime, domain, adapter, behavior, and utility projects.

## Dependency Rules

Test helpers may depend on production code; production code must not depend on test projects.

## Invariants

Do not treat a passing skipped physical test as evidence of real-hardware behavior.

## Architectural Constraints

Prefer deterministic unit/integration tests and `TimeProvider`; never run physical tests casually.

## Common Pitfalls

[Physical runs](../knowledge/pitfalls/physical-runs.md), [periodic loops in tests](../knowledge/pitfalls/periodic-loops-in-tests.md)

## Related ADRs

[Hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md), [Gating](../docs/adr/0003-gate-pull-requests-with-one-aggregate-check.md), [quarantine](../docs/adr/0004-quarantine-failing-tests-with-a-trait.md), [test tiers (proposed)](../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md)

## Related Findings

[Scheduler not steppable under a fake clock](../knowledge/findings/periodic-scheduler-not-steppable-under-fake-time.md).

## Related Root Causes

[Interpolation baseline drift](../knowledge/root-causes/interpolation-baseline-drift.md), [Test host crash](../knowledge/root-causes/test-host-crash-from-scheduler-failfast.md), [step angle](../knowledge/root-causes/step-angle-4094-counts.md), [empty leg posture map](../knowledge/root-causes/empty-leg-posture-map.md), [Clean-runner failures](../knowledge/root-causes/tests-failing-only-on-a-clean-runner.md)

## Related Lessons Learned

[Goal versus measured state](../knowledge/lessons-learned/goal-versus-measured-state.md), [Tests must discriminate](../knowledge/lessons-learned/tests-must-discriminate.md)

## Related Failed Attempts

[Measured-angle interpolation](../knowledge/failed-attempts/measured-angle-interpolation.md).
