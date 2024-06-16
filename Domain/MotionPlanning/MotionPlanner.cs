using Domain.Structures;

namespace Domain.MotionPlanning;

public abstract class MotionPlanner()
{
    public abstract Posture GetPosture();
    public abstract void SetPosture(Posture posture);
}