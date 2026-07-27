using RobotDomain.Motion;

namespace EllieMain.MotionPlanning;

public class ArticulatedSteeringEllieMotionPlanner(WheelDriver wheelDriver) : EllieMotionPlanner
{
    public void Start()
    {
        wheelDriver.Initialize(WheelId.FrontLeft, RobotDomain.Structures.RotationDirection.Reverse);
        wheelDriver.Initialize(WheelId.FrontRight, RobotDomain.Structures.RotationDirection.Forward);
    }
}