using LanguageExt;
using RobotDomain.Time;
using Duration = UnitsNet.Duration;

namespace RobotDomain.Motion;

/// <summary>
/// Continuously tracks a goal motion of a structure part, which may require 
/// nonlinear actuator subgoals
/// </summary>
/// <remarks>
/// For example, moving a leg tip straight down at a steady pace will require
/// some joints to move more in the beginning than others. Commanding the
/// structure to simply move the tip to the end in one go will mean the tip
/// does not move in a linear fashion towards the goal.
///
/// Or for an articulated steering robot, setting a new turning speed means
/// outer and inner wheels need to move at different speeds depending on the
/// articulation angle, not just change speeds linearly from now to goal time. 
/// </remarks>
/// <typeparam name="TMotion"></typeparam>
public abstract class PeriodicTrackingMotionPlanner<TMotion> : TrackingMotionPlanner<TMotion>, IDisposable
{
    Task? _trackingTask;
    public Option<Timed<TMotion>> Goal { get; set; } = Option<Timed<TMotion>>.None;
    public readonly Duration Period = Duration.FromSeconds(1);
    
    public void Start(CancellationToken ct)
    {
        _trackingTask = PeriodicScheduler.RunAsync(
            action: () => Goal.IfSome(TrackGoalOnce), 
            duration: Period, 
            ct);
    }

    // TODO turn into a delegate that is injected to Start(), instead of the
    //  class being abstract. Inheritance is not meant for reusing code, and
    //  the difference in behavior can be injected instead of abstract. It
    //  makes the class hierarchy simpler and more flexible.
    protected abstract void TrackGoalOnce(Timed<TMotion> goalMotionTimed);

    public void Dispose()
    {
        _trackingTask?.Dispose();
    }
}
