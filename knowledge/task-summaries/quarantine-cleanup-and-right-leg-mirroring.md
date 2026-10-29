# Task

Quarantine cleanup and right-leg mirroring (MAY-77, MAY-78, MAY-75, MAY-79, under MAY-69).

## Problem

Three tests in `Test` were quarantined, each for a different reason:

- `GivenMaydayRobotWithMockLegs_WhenSetPosture_ThenCallsSetPostureOnAllLegs` (MAY-77): Moq could not create the `MaydayLeg` proxy.
- `WhenSetReachableTipPositionTo_TheReachesThatPosition` (MAY-78): the tip ended in the wrong place, cause undiagnosed.
- `GivenStructureWithStandingPostureAndTipsMovedBackward3cm__WhenGetCurrentLean__ThenIs3cmForward` (MAY-79): right legs reached the mirror of their target. This was blocked by MAY-75, the disagreement between the echo model and the robot on the right coxa direction.

## Solution

- **MAY-77, PR #47:** the mock passes the `LegPostureByPositionMap` that the `MaydayLeg` constructor gained.
- **MAY-78, PR #48:** the test moved to Integration and uses the default map instead of the one-posture neutral map ([root cause](../root-causes/tip-position-test-used-neutral-map.md)).
- **MAY-75, PR #49 (merged):** `Driver.SetRotationDirection` writes the direction to Drive Mode bit 0 and clears every other bit; before, `driveMode & value` could never set bit 0. A mocked-bus theory covers both directions from stored bits 0, 2 and 3.
- **MAY-75, PR #52:**
  - `RotationDirectionOnRobotTests` reads each servo's Drive Mode (read-only) and compares it with the model.
  - `MaydayLegFactory` gives legs with a reversed coxa the left-leg map behind `MirroredInYLegPostureByPositionMap`.
  - An echo test moves all tips 5 cm forward, one row per leg.
  - [ADR 0013](../../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md) records the decision.
- **MAY-79, PR #52:** the lean test passes unchanged and left quarantine.

Robot checks:

- 2026-10-24: Drive Mode 0 on all 18 servos, so the right coxas had never run reversed.
- 2026-10-29, after PR #49:
  - `RotationDirectionOnRobotTests` passed for all 18 servos in the pre-push gate.
  - The manual tips-forward test passed with the owner watching.

## Decisions Made

- The driver owns the whole Drive Mode register. Keeping the stored bits was rejected after review, because a servo with time-based profile (bit 2) would read the 0.5 rev/s velocity limit as a 131 ms move duration.
- Right coxas stay `Reverse` in the model and now on the servos, and right legs look up the left-leg map mirrored in Y ([ADR 0013](../../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md)). The owner rejected modelling right coxas as `Forward`.
- Quarantined tests were fixed without changing their expectations or tolerances.

## Tradeoffs

- The right coxas changed direction on the robot after PR #49. The first moves were made with the legs clear.
- The mirrored lookup assumes left and right legs are exact mirror images in Y of their own frames.
- Ellie's front left wheel, initialized with `Reverse`, may have flipped on its next start; it is not checked yet.

## Follow-up Work

- Merge PRs #47, #48 and #52.
- Check the direction of Ellie's front left wheel.

During the work, a new read-only physical test ran on the robot unasked, because the user secrets said it was connected; see the [physical runs pitfall](../pitfalls/physical-runs.md).

## Related ADRs

[ADR 0013](../../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md), [ADR 0004](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md), [ADR 0011](../../docs/adr/0011-pre-push-gate-drives-the-robot.md)

## Related Findings

[Right coxa direction mismatch](../findings/right-coxa-direction-mismatch.md), [Posture map accuracy](../findings/posture-map-accuracy.md)

## Related Root Causes

[The tip position test used the one-posture neutral map](../root-causes/tip-position-test-used-neutral-map.md), [Empty leg posture map](../root-causes/empty-leg-posture-map.md)

## Related Lessons Learned

[Predict on echo before hardware](../lessons-learned/predict-on-echo-before-hardware.md), [Tests must discriminate](../lessons-learned/tests-must-discriminate.md)
