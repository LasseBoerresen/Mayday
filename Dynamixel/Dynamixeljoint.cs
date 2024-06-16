using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class DynamixelJoint : Joint
{
    public override JointState State { get; }
    public override void SetAngleGoal(Angle goal)
    {
        throw new NotImplementedException();
    }
}
