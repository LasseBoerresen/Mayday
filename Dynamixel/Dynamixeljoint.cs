using RobotDomain.Geometry;
using RobotDomain.Motion;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace Dynamixel;

// TODO Rename To and move out of dynamixel component, there is nothing dynamixly about it, other than it having a singleton  

/// <summary>
/// An individual rotary servo joint using a singleton servo 
/// </summary>
public class DynamixelJoint : Joint
{
    readonly JointId _id;
    readonly JointDriver _jointDriver;

    public DynamixelJoint(
        JointId id,
        JointDriver jointDriver,
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
        _jointDriver = jointDriver;
    }
    
    // TODO: create state proxy, that simply updates at a base frequency,
    //  to decouple queries from communication with dynamixel, and always
    //  just returns the current value.
    //  I could add a "boost" functionality, where whenever cache is hit, we
    //  double the frequency, but it decays on its own. 
    public override JointState State => _jointDriver.GetState(_id); 
    
    public override void SetAngleGoal(Timed<Angle> goal) => _jointDriver.SetGoalAngleFor(_id, goal);

    public void Initialize() => _jointDriver.Initialize(_id, RotationDirection);
}