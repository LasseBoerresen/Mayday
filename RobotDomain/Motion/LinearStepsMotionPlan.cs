using RobotDomain.Time;

namespace RobotDomain.Motion;

// TODO ensure steps are non-empty, ordered and unique in time. 
public record LinearStepsMotionPlan<TState>(IEnumerable<Timed<TState>> Steps) 
        : MotionPlan<TState>
{
    public Timed<TState> At(DateTimeOffset time)
    {
        var futureSteps = Steps.Where(s => s.ArrivalTime > time).ToList();
       
        return futureSteps.FirstOrDefault(defaultValue: Steps.Last());
    }
}
