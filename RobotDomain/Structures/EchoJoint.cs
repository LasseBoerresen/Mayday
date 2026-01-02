using RobotDomain.Geometry;
using RobotDomain.Physics;
using RobotDomain.Time;
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
    Timed<Angle> _goal = Timed<Angle>.Passed(Angle.Zero);
    Timed<Angle> _goalPrevious = Timed<Angle>.Passed(Angle.Zero);
    
    public override JointState State =>
        new(
            Angle: _goal.Target,
            RotationalSpeed: RotationalSpeed.Zero,
            Torque: LoadRatio.Zero,
            Temperature: Temperature.FromDegreesCelsius(23),
            AngleGoal: _goal,
            AngleGoalPrevious: _goalPrevious);

    public override void SetAngleGoal(Timed<Angle> goal)
    {
        _goalPrevious = _goal;
        _goal = goal;
    }
}