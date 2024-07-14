using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class DynamixelJoint : Joint
{
    readonly Pose _hornPose;
    readonly JointId _id;
    readonly Adapter _adapter;

    public DynamixelJoint(Link parent, Link child, Pose hornPose, JointId id, Adapter adapter) 
        : base(ComponentId.New, parent, child)
    {
        _hornPose = hornPose;
        _id = id;
        _adapter = adapter;
    }
    
    public override JointState State => _adapter.GetState(); 
    public override void SetAngleGoal(Angle goal) => _adapter.SetGoal(_id, new(goal));

    public void Initialize() => _adapter.Initialize(_id);
    
    public override Pose Pose => throw new NotImplementedException("Implement getting pose of child using angle and horn pose");
}
