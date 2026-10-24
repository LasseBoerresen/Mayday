# User secrets overrode the gate's robot-disconnected flag

## Symptoms

On 2026-10-24 a Release run of `scripts\gate.ps1` reported 10 failed, 230 passed and 5 skipped tests, while the same build run directly reported 230 passed and 8 skipped. Among the failures was `NativeSerialPortCommunicationBusTests.GivenRobotIsAttached_WhenSetTwoGoalAnglesAndSleep1sBetween_ThenCurrentPositionWithin20`, a `[PhysicalRobotFact]`. It ran for 3 s, enabled torque on servos 1–18 and commanded goal positions, then failed with "goal not reached". The user confirmed a robot was attached to the port.

## Investigation

- The gate sets `$env:MAYDAY_ROBOT_IS_CONNECTED = 'False'` before any step, so physical tests were expected to skip.
- `Test/Utilities/TestConfiguration.Create()` called `.AddEnvironmentVariables()` and then `.AddUserSecrets<TestConfiguration>(optional: true)`. In `Microsoft.Extensions.Configuration` the source added last wins.
- The user secrets file was rewritten at 13:39:45, after the gate run, when the user set `MAYDAY_ROBOT_IS_CONNECTED` to `false` and disconnected the port. With the secret `True` during the run, the gate's environment value was overridden.
- A unit test with in-memory sources (secret `True`, environment `False`) returned connected under the old order: 2 of its 3 cases failed.
- The order dates from `f6b788e`, which made the secrets optional and kept them last.

## Root Cause

The configuration added user secrets after environment variables, so the secret decided whenever both were set. The gate's only guard against physical tests was that environment variable, so a developer secret set to `True` turned a gate run into a physical run.

## Resolution

- `506163e`: `TestConfiguration` adds the environment last, so it overrides the secrets. `TestConfigurationTests` pins the order with in-memory sources.
- `570ade9`: `PhysicalRobotFact` and `PhysicalRobotTheory` carry the trait `Tier=Physical` ([ADR 0005](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md)). The gate filters on `Tier!=Physical` in the blocking and the quarantined run, and a Pester test checks that filter. The flag stays as a second, independent guard.

## Prevention

Protect hardware with a guard that does not depend on configuration (the trait filter), and keep the configuration guard as a second layer. When a caller relies on an environment variable overriding a setting, test the source order. See the [physical-runs pitfall](../pitfalls/physical-runs.md).

## Related Components

[Test](../../Test/README.md), [scripts](../../scripts/README.md), [Dynamixel](../../Dynamixel/README.md)

## Related Tasks

None recorded.

## Related Decisions

[ADR 0005](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md), [ADR 0006](../../docs/adr/0006-run-quality-gates-locally.md)
