# Problem

## Symptoms

- In the manual tips-forward test, moving all tips 9 cm forward from `SittingTall` made the rear tips go down instead of forward. The owner saw the rear femur motor hit the body.
- In the original `SittingTall` posture (femur −0.3 revolutions), the femur motor touched the body as well.

## Investigation

- On the echo model, the 9 cm move takes the rear legs from coxa 0°, femur −90°, tibia −40° to coxa ±37°, femur −129°, tibia −66°. The rear legs point backwards, so "forward" pulls their tips about 3.5 cm towards their mounts, and the femur folds down to stay at the same height.
- The model predicts the rear tip stays within 0.5 cm of its height along the whole move, so the downward movement on the robot comes from the collision, not from the commanded postures.
- `JointLimits.Defaults` allows femur angles down to −0.4 revolutions (−144°), independently of the coxa and tibia. `LegPostureByPositionMapDictImpl.BuildNew` samples every posture inside those limits, so the map contains colliding postures.

## Root Cause

Nothing in the software models the body. Joint limits are per-joint ranges, and the posture map includes every posture within them, so a lookup can return a posture whose femur motor hits the body.

## Resolution

- `3253c99`: `SittingTall` unfolds the femur to −0.25 revolutions, and `fe35f89` turns the tibia up to −0.11 revolutions so the tips stay raised (1.5 cm above the thorax origin).
- `02011a4`: the manual test moves 5 cm, where the rear femur stays clear of the body, instead of 9 cm.
- Keeping postures clear of the body in general is planned under MAY-76.

## Prevention

Do not rely on joint limits for collision safety. When a motion pulls tips towards the body, check the commanded femur angles on the echo model before a physical run, and watch the first run closely.

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [Test](../../Test/README.md)

## Related Tasks

[Lean test and manual robot tests](../task-summaries/lean-test-and-manual-robot-tests.md)

## Related Decisions

[ADR 0009](../../docs/adr/0009-manual-robot-tests-confirmed-by-a-human.md)
