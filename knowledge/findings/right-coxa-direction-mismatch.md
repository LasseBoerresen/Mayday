# Finding

## Description

The echo model and the real robot disagreed on which way the right legs move. The cause is confirmed: the right coxa servos never ran reversed, although the model reverses them. It is resolved by PR #49 and [ADR 0013](../../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md) (PR #52).

## Why It Matters

Until the model, the servos and the posture lookup agree on each joint's direction, simulated motions do not predict the real robot on the right side, and anything computed from right-leg joint angles (tip positions, `GetCurrentLean`) is mirrored there whenever a coxa is not at 0°.

## Evidence

Before the fix:

- `MaydayLegFactory.CreateCoxaMotorToCoxaJoint` gives right legs `RotationDirection.Reverse`; `Joint.Angle` multiplies the joint angle by it. Femur and tibia are `Forward` on both sides.
- The posture map is built from the left rear leg only (`LegPostureByPositionMapDictImpl.CreateEchoLeg`). With a reversed coxa, a posture from the map puts a right tip at the left tip's position mirrored in Y in the leg's own frame.
- On the echo model, moving all tips 5 cm forward from `SittingTall` moved the right tips by (−0.007, −0.049, 0.007) m (front), (−0.050, 0, 0.004) m (center) and (−0.005, 0.046, −0.006) m (rear); the left tips moved about 5 cm forward.
- On the robot, in the manual tips-forward test (PR #45), the owner saw the right center and right front legs move forward as expected.
- `Driver.SetRotationDirection` computed `driveMode & value`, with `Forward` = 0 and `Reverse` = 1. That can never set the reverse bit (bit 0).
- On 2026-10-24, `RotationDirectionOnRobotTests` (read-only) read Drive Mode 0 on all 18 servos, right coxas 10, 13 and 16 included. So every coxa ran the normal way, and the unreversed right coxas cancelled the left-leg map's mirroring on the robot. The owner's report that the right coxa servos were configured to rotate the other way is not reflected in their registers.

After the fix:

- PR #49: the driver writes the direction alone to Drive Mode (bit 0 set for `Reverse`, every other bit cleared, as before).
- PR #52: `MaydayLegFactory` gives legs with a reversed coxa the left-leg map behind `MirroredInYLegPostureByPositionMap`. The echo tips-forward test passes for all six legs, and the 3 cm lean test leaves quarantine.
- On 2026-10-29, in the pre-push gate on the robot, `RotationDirectionOnRobotTests` passed for all 18 servos, and the manual tips-forward test passed with the owner watching: all six tips moved about 5 cm forward.

## Recommendation

Keep the model, the servos and the lookup on one convention ([ADR 0013](../../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md)); `RotationDirectionOnRobotTests` checks the servos against the model in every code push. Ellie uses the same driver and initializes its front left wheel with `Reverse`; if that wheel's EEPROM lacked bit 0, it turns the other way from its next start since PR #49, so check its direction before relying on it.

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [Dynamixel](../../Dynamixel/README.md)

## Related Decisions

[ADR 0013](../../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md), [ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
