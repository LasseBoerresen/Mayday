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
- `Driver.SetRotationDirection` computes `driveMode & value`, with `Forward` = 0 and `Reverse` = 1. That can never set the reverse bit (bit 0); it only keeps it when the servo already has it, and `Forward` clears every Drive Mode bit. Fixed under MAY-75: it now writes the direction to bit 0 and clears every other bit, as before (unit-tested with a mocked bus, not yet run on the robot).

Hypothesis, not confirmed: the servos' actual direction differs from what the model assumes, through the `&` in `SetRotationDirection` or the servos' stored settings, and it happens to cancel the left-leg map's mirroring on the robot. Reading the right coxas' Drive Mode register on the robot would settle it.

Because the fix makes `Reverse` take effect even where the servo's EEPROM lacked bit 0, the right coxas may turn the other way on the next robot start than in PR #45. If the hypothesis holds, the robot will then match the echo model, right tips included.

## Recommendation

Do not trust echo-model predictions for right legs off coxa 0° until MAY-75 is resolved: read the servos' direction on the robot, and make the model and the lookup agree with it. Before the first physical run after the Drive Mode fix, predict the motion on the echo model and expect the right coxas to follow it rather than the earlier robot run. A servo whose direction flips also reports its position mirrored around the center step, so a wrong direction can swing a right coxa through a large arc on the first goal: run that first move with the legs clear or the robot lifted. Ellie uses the same driver and initializes its front left wheel with `Reverse`, so that wheel may flip as well on its next start.

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [Dynamixel](../../Dynamixel/README.md)

## Related Decisions

[ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
