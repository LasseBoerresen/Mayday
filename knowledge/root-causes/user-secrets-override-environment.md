# Problem

## Symptoms

- Running the manual test with `$env:MAYDAY_ROBOT_IS_CONNECTED='True'` skipped it with "Robot is not connected", although the variable was set.
- The gate forces `$env:MAYDAY_ROBOT_IS_CONNECTED = 'False'` to keep physical tests off the robot, but its own script test only checked the variable it passes on, not what the test project reads.

## Investigation

- The skip reason, read from a test results file, was the robot-not-connected message.
- `TestConfiguration.Create()` adds environment variables first and user secrets second, and later sources win in `Microsoft.Extensions.Configuration`. The Test project's user secrets held `MAYDAY_ROBOT_IS_CONNECTED = False`, which overrode the environment.
- With the secret set to `True` (as it was while the owner prepared a physical run), the gate's forced `False` would lose the same way, and the Debug gate the Claude Code Stop hook runs would have driven the robot with every physical test.

## Root Cause

User secrets override environment variables in `TestConfiguration`, so an environment variable cannot switch a physical test off, or on, when the secret says otherwise.

## Resolution

- `5750818`: physical tests carry the trait `Tier=Physical`, and the gate (and CI) exclude `Tier=Physical` and `Tier=Manual` through the test filter, whatever the configuration says. The forced variable stays as a second guard. A script test fails if the gate's filter loses either exclusion.
- The source order in `TestConfiguration` is unchanged: to run a physical or manual test, set the keys in the user secrets.

## Prevention

Keep safety-critical exclusions in the test selection, not in configuration that another source can override. When a run depends on a configuration value, check which source wins.

## Related Components

[Test](../../Test/README.md), [scripts](../../scripts/README.md)

## Related Tasks

[Lean test and manual robot tests](../task-summaries/lean-test-and-manual-robot-tests.md)

## Related Decisions

[ADR 0005](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md), [ADR 0006](../../docs/adr/0006-run-quality-gates-locally.md), [ADR 0009](../../docs/adr/0009-manual-robot-tests-confirmed-by-a-human.md)
