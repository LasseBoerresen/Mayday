using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class DynamixelJoint : Joint
{
    readonly JointId _id;
    readonly Adapter _adapter;

    public DynamixelJoint(
        JointId id,
        Adapter adapter,
        Transform passiveTransform,
        RobotDomain.Structures.RotationDirection rotationDirection,
        AttachmentOrder attachmentOrder,
        Link parent,
        Link child) 
        : base(
            passiveTransform,
            rotationDirection,
            attachmentOrder,
            ComponentId.New, 
            parent, 
            child)
    {
        _id = id;
        _adapter = adapter;
    }
    
    // TODO: create state proxy, that simply updates at a base frequency,
    //  to decouple queries from communication with dynamixel, and always
    //  just returns the current value.
    //  I could add a "boost" functionality, where whenever cache is hit, we
    //  double the frequency, but it decays on its own. 
    public override JointState State => _adapter.GetState(_id); 
    
    public override void SetAngleGoal(Angle goal) => _adapter.SetGoalAngleFor(_id, goal);

    public void Initialize() => _adapter.Initialize(_id, RotationDirection);
}