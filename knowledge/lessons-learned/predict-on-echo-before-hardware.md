# Lesson

## Situation

The manual tips-forward test (PR #45) was the first time a tip-position motion was checked on the robot by eye. Before each hardware run, the same motion was run on an echo copy of the structure with the same posture map, and the predicted per-leg movement was written down.

## Observation

- The prediction for `Sitting` said both rear legs would not move, because the map had no posture for their targets. The robot did exactly that, so the cause was known before anyone looked at the hardware.
- The prediction also said the right center tip would move backward, and the robot moved it forward. The disagreement is what exposed the model's right-coxa mismatch ([finding](../findings/right-coxa-direction-mismatch.md)).
- Moves that the echo model showed were partly unreachable (6 cm and 10 cm from `SittingTall`) were never sent to the robot.
- What the echo model cannot show is physical contact: the rear femur motor hitting the body at 9 cm only showed on the robot ([root cause](../root-causes/rear-femur-hits-body.md)).

## Recommendation

Before a physical run, run the same motion on the echo model and record the expected movement of each leg. A run that matches confirms the model; a mismatch is a finding about the model or the hardware, not noise. Use the echo run to choose start postures and move sizes every leg can reach, and still watch the first run on the robot for collisions.

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [Test](../../Test/README.md)

## Related Tasks

[Lean test and manual robot tests](../task-summaries/lean-test-and-manual-robot-tests.md)
