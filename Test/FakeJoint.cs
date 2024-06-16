using Domain.Structures;
using UnitsNet;

namespace Test;

public class FakeJoint(Angle angle) : Joint
{
    public override JointState State => new(
        angle,
        RotationalSpeed.Zero,
        Torque.Zero,
        UnitsNet.Temperature.Zero,
        AngleGoal: Angle.Zero);
}