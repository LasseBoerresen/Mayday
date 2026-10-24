# Task

## Problem

MAY-71: `GivenStructureWithStandingPostureAndTipsMovedBackward1cm__WhenGetCurrentLean__ThenIs1cmForward` was quarantined with an undiagnosed lean of 0. Diagnosing it raised questions about the posture map and the right legs that only the real robot could answer, and there was no test that could check the robot's movement.

## Solution

- PR #44 (`cdf9d78`): the lean test uses the robot's real posture map and leaves quarantine at a 3 cm tolerance.
- PR #45:
  - `fcd1487`: a manual tier, `[ManualRobotFact]` with trait `Tier=Manual`, asserted by the watching human through `HumanVerifier` and a prompt window ([ADR 0009](../../docs/adr/0009-manual-robot-tests-confirmed-by-a-human.md)).
  - `0bc1e8f`, `aa663a6`, `02011a4`: the manual test sits tall, confirms straight legs, moves all tips 5 cm forward and asks whether all six moved forward. The owner confirmed on the robot that it does.
  - `5750818`: physical tests carry `Tier=Physical`, and the gate and CI exclude physical and manual tests ([ADR 0005](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md) accepted, partly implemented).
  - `3253c99`, `fe35f89`: `SittingTall` is (0, −0.25, −0.11) revolutions, clear of the body with raised tips.
- `78c4d24`: CLAUDE.md describes how Linear issues are written.

## Decisions Made

- Accept a 3 cm tolerance in the lean test for now (owner), with analytical IK planned instead of tuning the map.
- Assert physical behavior by a human answer in a new tier, rather than a manual behavior with no pass or fail.
- Keep the robot out of the gate through the test filter, rather than by reordering configuration sources.

## Tradeoffs

- At 3 cm, the lean test no longer tells a 1 cm lean from none; it waits for MAY-74.
- Manual tests need a person at the robot and run only on Windows.

## Follow-up Work

- MAY-74: analytical inverse kinematics with sub-mm accuracy, alongside the map.
- MAY-75: the model's right coxa direction matches the servos.
- MAY-76: postures never hit the body.
- `TestConfiguration` still lets user secrets override environment variables.

## Related ADRs

[ADR 0005](../../docs/adr/0005-test-tiers-for-physical-and-simulated-runs.md), [ADR 0009](../../docs/adr/0009-manual-robot-tests-confirmed-by-a-human.md)

## Related Findings

[Posture map accuracy](../findings/posture-map-accuracy.md), [right coxa direction mismatch](../findings/right-coxa-direction-mismatch.md)

## Related Root Causes

[Empty leg posture map](../root-causes/empty-leg-posture-map.md), [rear femur hits the body](../root-causes/rear-femur-hits-body.md), [user secrets override the environment](../root-causes/user-secrets-override-environment.md)

## Related Lessons Learned

[Predict on echo before hardware](../lessons-learned/predict-on-echo-before-hardware.md), [tests must discriminate](../lessons-learned/tests-must-discriminate.md)
