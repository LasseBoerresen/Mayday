using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class DynamixelJoint : Joint
{
    readonly JointId _id;
    readonly Adapter _adapter;

    public DynamixelJoint(Link parent, Link child, JointId id, Adapter adapter) 
        : base(ComponentId.New, parent, child)
    {
        _id = id;
        _adapter = adapter;
    }

    public override JointState State => _adapter.GetState(); 
    public override void SetAngleGoal(Angle goal) => _adapter.SetGoal(_id, new(goal));

    public void Initialize() => _adapter.Initialize(_id);
}
