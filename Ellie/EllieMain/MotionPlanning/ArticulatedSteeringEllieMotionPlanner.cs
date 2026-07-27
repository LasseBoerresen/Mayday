using RobotDomain.Motion;

namespace EllieMain.MotionPlanning;

public class ArticulatedSteeringEllieMotionPlanner(WheelController wheelController) : EllieMotionPlanner
{
    public void Start()
    {
        wheelController.Initialize(WheelId.FrontLeft, RobotDomain.Structures.RotationDirection.Reverse);
        wheelController.Initialize(WheelId.FrontRight, RobotDomain.Structures.RotationDirection.Forward);
    }
}