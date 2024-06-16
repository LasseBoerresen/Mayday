using RobotDomain.Structures;

namespace RobotDomain.MotionPlanning;

public abstract class MotionPlanner()
{
    public abstract Posture GetPosture();
    public abstract void SetPosture(Posture posture);
}