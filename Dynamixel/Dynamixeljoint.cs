using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class DynamixelJoint : Joint
{
    readonly Pose _passivePose;
    readonly JointId _id;
    readonly Adapter _adapter;

    public DynamixelJoint(Link parent, Link child, Pose passivePose, JointId id, Adapter adapter) 
        : base(ComponentId.New, parent, child)
    {
        _passivePose = passivePose;
        _id = id;
        _adapter = adapter;
    }
    
    public override JointState State => _adapter.GetState(); 
    public override void SetAngleGoal(Angle goal) => _adapter.SetGoal(_id, new(goal));

    public void Initialize() => _adapter.Initialize(_id);
    
    public override Pose Pose => _passivePose + ActivePose;

    Pose ActivePose => Pose.FromQ(Q.FromRpy(new(Angle.Zero, Angle.Zero, State.Angle)));
}
