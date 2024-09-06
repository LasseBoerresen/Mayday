using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class DynamixelJoint : Joint
{
    readonly Transform _passiveTransform;
    readonly JointId _id;
    readonly Adapter _adapter;

    public DynamixelJoint(Link parent, Link child, Transform passiveTransform, JointId id, Adapter adapter) 
        : base(ComponentId.New, parent, child)
    {
        _passiveTransform = passiveTransform;
        _id = id;
        _adapter = adapter;
    }
    
    public override JointState State => _adapter.GetState(_id); 
    public override void SetAngleGoal(Angle goal) => _adapter.SetGoal(_id, new(goal));

    public void Initialize() => _adapter.Initialize(_id);

    protected override Transform Transform => _passiveTransform + ActiveTransform;

    Transform ActiveTransform => Transform.FromQ(Q.FromRpy(new(Angle.Zero, Angle.Zero, State.Angle)));
}
