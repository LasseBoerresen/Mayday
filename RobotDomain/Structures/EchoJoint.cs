using RobotDomain.Geometry;
using RobotDomain.Physics;
using UnitsNet;

namespace RobotDomain.Structures;

public class EchoJoint(
    Transform passiveTransform,
    RotationDirection rotationDirection,
    AttachmentOrder attachmentOrder,
    ComponentId id, 
    Link parent, 
    Link child) 
    : Joint(
        passiveTransform,
        rotationDirection,
        attachmentOrder,
        id, 
        parent, 
        child)
{
    Angle _goal = Angle.Zero;
    
    public override JointState State =>
        new(
            Angle: _goal,
            RotationalSpeed: RotationalSpeed.Zero,
            Torque: LoadRatio.Zero,
            Temperature: Temperature.FromDegreesCelsius(23),
            AngleGoal: _goal);

    public override void SetAngleGoal(Angle goal) => _goal = goal;
}