# Finding

## Description

The echo model and the real robot disagree on which way the right legs move. The cause is not confirmed.

## Why It Matters

Simulated motions do not predict the real robot on the right side, and anything computed from right-leg joint angles (tip positions, `GetCurrentLean`) is mirrored there whenever a coxa is not at 0°.

## Evidence

- `MaydayLegFactory.CreateCoxaMotorToCoxaJoint` gives right legs `RotationDirection.Reverse`; `Joint.Angle` multiplies the joint angle by it. Femur and tibia are `Forward` on both sides.
- The posture map is built from the left rear leg only (`LegPostureByPositionMapDictImpl.CreateEchoLeg`). With a reversed coxa, a posture from the map puts a right tip at the left tip's position mirrored in Y in the leg's own frame.
- On the echo model, moving all tips forward gives the right legs the opposite coxa sign to the left legs (for example +5.6° against −5.6°), which with `Reverse` turns them the same physical way: the right center tip moves backward (−4.5 cm for a 5 cm move).
- On the robot, in the manual tips-forward test (PR #45), the owner saw the right center and right front legs move forward as expected.
- The owner reports that the right coxa servos are configured to rotate the other way.
- `Driver.SetRotationDirection` computes `driveMode & value`, with `Forward` = 0 and `Reverse` = 1. That can never set the reverse bit (bit 0); it only keeps it when the servo already has it, and `Forward` clears every Drive Mode bit.

Hypothesis, not confirmed: the servos' actual direction differs from what the model assumes, through the `&` in `SetRotationDirection` or the servos' stored settings, and it happens to cancel the left-leg map's mirroring on the robot. Reading the right coxas' Drive Mode register on the robot would settle it.

## Recommendation

Do not trust echo-model predictions for right legs off coxa 0° until MAY-75 is resolved: fix the Drive Mode bit handling, read the servos' direction on the robot, and make the model and the lookup agree with it.

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [Dynamixel](../../Dynamixel/README.md)

## Related Decisions

[ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
