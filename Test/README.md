# Test

## System Overview

The main xUnit project of the repository: unit and integration tests of the
Mayday and shared-layer projects, and the test infrastructure that decides
which tests may move the real robot. `Ellie/EllieMainTests` references this
project only for the test doubles `TestTerminal` and
`RecordingFatalErrorHandler`; it uses no tier attribute, so its tests are not
gated by `Tier`.

## Subdirectories & System Boundaries

| Folder | What it holds |
|---|---|
| [Unit](./Unit/README.md) | One collaborator at a time, hardware-free except one FTDI latency call; shared assertions `TestObjectFactory` and `TestId`. |
| [Integration](./Integration/README.md) | Several real components together: echo-joint tests with the default posture map, and the physical and manual tests that move the robot. |
| [Utilities](./Utilities/README.md) | The tier attributes (`PhysicalRobotFact`, `PhysicalRobotTheory`, `ManualRobotFact`, `Quarantine`), `TestConfiguration`, `HumanVerifier`, `RecordingFatalErrorHandler`, `TestTerminal`. |

Each folder under `Unit` and `Integration` mirrors a production project and
links its own README.

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

### Global Invariants

- A test that may move the robot carries `Tier=Physical` or `Tier=Manual`
  through its attribute; the gates select tests by these traits and by
  `Quarantine=true`. The trait discoverers are named by string in
  `Utilities`, so renaming or moving them drops the traits silently.
- `TestConfiguration` reads environment variables, then the user secrets of
  `Test.csproj` (`UserSecretsId` `de615b0d-591a-44e6-8040-6f9239d5d5dc`), and
  the secrets win. A physical test runs only when
  `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE` is exactly `True` (case-sensitive), a manual
  test only when `MAYDAY_HUMAN_IS_PRESENT` is also exactly `True`. The skip
  message for the first flag says `'true'`; only `True` passes.
- The attributes check only these flags, not whether the robot is connected.
- Most physical tests never release the serial adapter, so the pre-push gate
  runs each physical test method in its own process.
- Hardware-free tests that place tips use the default posture map, not the
  neutral one (see
  [Integration/MaydayDomain/Base](./Integration/MaydayDomain/Base/README.md)).
- Warnings are errors in Debug and Release.
- `Test.csproj` declares the folders `Integration\Base` and
  `Unit\MaydayDomain\Structures` (`<Folder Include=…>`), but neither exists;
  the object mothers live in each area's own `Base` folder.

### Global Side Effects

- Physical and manual tests open the FTDI adapter (serial number `FT2H2YMW`)
  and its COM port, enable torque and move the robot; manual tests open a
  PowerShell prompt window.
- Building any `MaydayRobotFactory` robot effect opens and closes the FTDI
  adapter when it is plugged in, even when the effect is never run.
- Any test that uses `PeriodicScheduler` sets the whole test process to high
  priority and 1 ms Windows timer resolution.
- `Unit/MaydayDataAccess/TestData/*.json` are copied to the output directory
  and read from there; `DefaultLegPostureByPositionMap.json` comes from
  `MaydayDataAccess`.

### System Verification

- Run without moving the robot (still reaches the FTDI adapter, see above): `dotnet test Test\Test.csproj --filter "Tier!=Physical&Tier!=Manual&Quarantine!=true"`,
  or `scripts\gate.ps1`, which also forces the robot to not clear-to-move.
- Physical and manual runs move the robot: only after the operator confirms
  the robot, port and operating conditions.

## Architectural Constraints

Prefer deterministic unit/integration tests and `TimeProvider`; never run physical tests casually.

## Common Pitfalls

[Physical runs](../knowledge/pitfalls/physical-runs.md), [periodic loops in tests](../knowledge/pitfalls/periodic-loops-in-tests.md), [posture map silent fallback](../knowledge/pitfalls/posture-map-silent-fallback.md)

## Related ADRs

[Hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md), [Local gate](../docs/adr/0006-run-quality-gates-locally.md) (replaces [aggregate-check gating](../docs/adr/0003-gate-pull-requests-with-one-aggregate-check.md)), [quarantine](../docs/adr/0004-quarantine-failing-tests-with-a-trait.md), [test tiers](../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md), [manual robot tests](../docs/adr/0009-manual-robot-tests-confirmed-by-a-human.md)

## Related Findings

[Scheduler not steppable under a fake clock](../knowledge/findings/periodic-scheduler-not-steppable-under-fake-time.md), [posture map accuracy](../knowledge/findings/posture-map-accuracy.md).

## Related Root Causes

[Interpolation baseline drift](../knowledge/root-causes/interpolation-baseline-drift.md), [Test host crash](../knowledge/root-causes/test-host-crash-from-scheduler-failfast.md), [step angle](../knowledge/root-causes/step-angle-4094-counts.md), [empty leg posture map](../knowledge/root-causes/empty-leg-posture-map.md), [Clean-runner failures](../knowledge/root-causes/tests-failing-only-on-a-clean-runner.md), [user secrets override the environment](../knowledge/root-causes/user-secrets-override-environment.md), [rear femur hits the body](../knowledge/root-causes/rear-femur-hits-body.md), [physical tests leak the serial adapter](../knowledge/root-causes/physical-tests-leak-the-serial-adapter.md), [tip position test used the neutral map](../knowledge/root-causes/tip-position-test-used-neutral-map.md)

## Related Lessons Learned

[Goal versus measured state](../knowledge/lessons-learned/goal-versus-measured-state.md), [Tests must discriminate](../knowledge/lessons-learned/tests-must-discriminate.md), [Predict on echo before hardware](../knowledge/lessons-learned/predict-on-echo-before-hardware.md), [Ratchet by pattern reading](../knowledge/lessons-learned/ratchet-by-pattern-reading-has-a-long-tail.md)

## Related Failed Attempts

[Measured-angle interpolation](../knowledge/failed-attempts/measured-angle-interpolation.md).
