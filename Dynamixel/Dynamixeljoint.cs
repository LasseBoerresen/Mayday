using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class DynamixelJoint(JointId id, Adapter adapter) : Joint
{
    public override JointState State { get; }
    public override void SetAngleGoal(Angle goal) => adapter.SetGoal(id, new(goal));
}
