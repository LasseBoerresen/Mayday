using RobotDomain.Time;

namespace RobotDomain.Motion;

public record LinearStepsMotionPlan<TState>(IEnumerable<Timed<TState>> Steps) 
        : MotionPlan<TState>
{
    public Timed<TState> At(DateTimeOffset time)
    {
        throw new NotImplementedException();
    }
}
