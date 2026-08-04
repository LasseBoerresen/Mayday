using LanguageExt;
using RobotDomain.Time;

namespace RobotDomain.Motion;

public interface TrackingMotionPlanner<TMotion> : MotionPlanner
{
    public void Start(CancellationToken ct);
    
    public Option<Timed<TMotion>> Goal { get; set; }
}
