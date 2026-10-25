# Test

## Responsibility

Mayday and shared-layer xUnit unit/integration tests and test utilities.

## Public APIs

`Unit`, `Integration`, and `Utilities` include test mothers, fake joints, terminal, and physical-test attributes.

## Key Concepts

Hardware-independent tests by default; physical tests explicitly marked `PhysicalRobotFact`/`PhysicalRobotTheory`, which carry the trait `Tier=Physical`. The Claude Code Stop hook's gate excludes `Tier=Physical` and `Tier=Manual`. The pre-push gate runs `Tier=Physical` on the robot, each test method in its own process, and fails on any skipped test outside `Tier=Manual` ([ADR 0011](../docs/adr/0011-pre-push-gate-drives-the-robot.md)). By hand: `dotnet test Test\Test.csproj --filter "Tier=Physical"`.

Manual tests (`[ManualRobotFact]`, trait `Tier=Manual`) drive the real robot and ask the person watching a yes/no question through `Utilities/HumanVerifier.cs`, which opens a prompt window (`Utilities/HumanPrompt.cs`, Windows only). They skip unless both `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE` and `MAYDAY_HUMAN_IS_PRESENT` are `True`; run them with `dotnet test Test\Test.csproj --filter "Tier=Manual"`. See [ADR 0009](../docs/adr/0009-manual-robot-tests-confirmed-by-a-human.md).

A test known to fail is marked `[Quarantine("reason")]` (`Utilities/QuarantineAttribute.cs`), on the method or the class. It gets the trait `Quarantine=true`, so `dotnet test --filter "Quarantine!=true"` is the blocking run and `--filter "Quarantine=true"` lists what is still broken. Quarantine records a failure with its cause; it never changes what a test expects. Fix the test or the code, then remove the attribute: the pre-push gate fails while a quarantined test passes.

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

[Physical runs](../knowledge/pitfalls/physical-runs.md), [periodic loops in tests](../knowledge/pitfalls/periodic-loops-in-tests.md), [posture map silent fallback](../knowledge/pitfalls/posture-map-silent-fallback.md)

## Related ADRs

[Hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md), [Local gate](../docs/adr/0006-run-quality-gates-locally.md) (replaces [aggregate-check gating](../docs/adr/0003-gate-pull-requests-with-one-aggregate-check.md)), [quarantine](../docs/adr/0004-quarantine-failing-tests-with-a-trait.md), [test tiers](../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md), [manual robot tests](../docs/adr/0009-manual-robot-tests-confirmed-by-a-human.md)

## Related Findings

[Scheduler not steppable under a fake clock](../knowledge/findings/periodic-scheduler-not-steppable-under-fake-time.md), [posture map accuracy](../knowledge/findings/posture-map-accuracy.md).

## Related Root Causes

[Interpolation baseline drift](../knowledge/root-causes/interpolation-baseline-drift.md), [Test host crash](../knowledge/root-causes/test-host-crash-from-scheduler-failfast.md), [step angle](../knowledge/root-causes/step-angle-4094-counts.md), [empty leg posture map](../knowledge/root-causes/empty-leg-posture-map.md), [Clean-runner failures](../knowledge/root-causes/tests-failing-only-on-a-clean-runner.md), [user secrets override the environment](../knowledge/root-causes/user-secrets-override-environment.md), [rear femur hits the body](../knowledge/root-causes/rear-femur-hits-body.md)

## Related Lessons Learned

[Goal versus measured state](../knowledge/lessons-learned/goal-versus-measured-state.md), [Tests must discriminate](../knowledge/lessons-learned/tests-must-discriminate.md), [Predict on echo before hardware](../knowledge/lessons-learned/predict-on-echo-before-hardware.md)

## Related Failed Attempts

[Measured-angle interpolation](../knowledge/failed-attempts/measured-angle-interpolation.md).
