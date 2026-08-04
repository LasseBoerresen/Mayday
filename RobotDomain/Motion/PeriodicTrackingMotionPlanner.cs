using LanguageExt;
using RobotDomain.Time;
using Duration = UnitsNet.Duration;

namespace RobotDomain.Motion;

public abstract class PeriodicTrackingMotionPlanner<TMotion> : TrackingMotionPlanner<TMotion>, IDisposable
{
    Task? _trackingTask;
    public Option<Timed<TMotion>> Goal { get; set; } = Option<Timed<TMotion>>.None;
    
    public void Start(CancellationToken ct)
    {
        _trackingTask = PeriodicScheduler.RunAsync(
            action: () => Goal.IfSome(TrackGoalOnce), 
            duration: Duration.FromSeconds(1), 
            ct);
    }

    protected abstract void TrackGoalOnce(Timed<TMotion> goalMotionTimed);

    public void Dispose()
    {
        _trackingTask?.Dispose();
    }
}
