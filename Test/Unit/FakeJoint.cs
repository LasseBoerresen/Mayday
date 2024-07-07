﻿using RobotDomain.Structures;
using UnitsNet;

namespace Test.Unit;

public class FakeJoint(Angle angle) 
    : Joint(ComponentId.New, Link.New, Link.New)
{
    public override JointState State => new(
        angle,
        RotationalSpeed.Zero,
        Torque.Zero,
        UnitsNet.Temperature.Zero,
        AngleGoal: Angle.Zero);

    public override void SetAngleGoal(Angle goal)
    {
        throw new NotImplementedException();
    }
}