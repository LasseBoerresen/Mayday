using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Motion;
using RobotDomain.Time;
using UnitsNet;
using Duration = UnitsNet.Duration;

namespace EllieMain.MotionPlanning;

/// <summary>
/// asdf
/// </summary>
/// <remarks>
/// Setting a new turning speed means outer and inner wheels need to move at
/// different speeds depending on the articulation angle, not just change
/// speeds linearly from now to goal time. 
/// </remarks>
/// <param name="wheelDriver"></param>
public class ArticulatedSteeringEllieMotionPlanner(WheelDriver wheelDriver) 
    : EllieMotionPlanner
{
    public Option<Timed<Motion>> Goal
    {
        get;
        set
        {
            field = value;
            
            // TODO make better steps than just the single goal value. Start at
            //   current state, and split in duration steps. 
            Plan = value.Map(MotionPlan<Motion> (tg) => new LinearStepsMotionPlan<Motion>([tg]));
        }
    } = Option<Timed<Motion>>.None;

    public Option<MotionPlan<Motion>> Plan { get; private set; } = Option<MotionPlan<Motion>>.None;

    Task? _trackingTask;
    readonly Duration Period = Duration.FromSeconds(1);
    
    public void Start(CancellationToken ct)
    {
        wheelDriver.Initialize(WheelId.FrontLeft, RobotDomain.Structures.RotationDirection.Reverse);
        wheelDriver.Initialize(WheelId.FrontRight, RobotDomain.Structures.RotationDirection.Forward);

        Action trackingAction = () => Plan.IfSome(TrackGoalOnce);
        
        _trackingTask = PeriodicScheduler.RunAsync(
            action: trackingAction, 
            duration: Period, 
            ct);
    }
    
    /// <summary>
    /// To be used for tracking a single motion. 
    /// </summary>
    /// <param name="goalMotionTimed"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void TrackGoalOnce(MotionPlan<Motion> goalMotionTimed)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Accelerate the robot by the given speed from its current speed
    /// </summary>
    /// <param name="timedSpeedChange"></param>
    public void AccelerateBy(Timed<Speed> timedSpeedChange)
    {
        var newGoal = timedSpeedChange.Map(newSpeed => 
            Goal
                .Some(goal => goal.Target with {ForwardSpeed = newSpeed }) // TODO Test that two accelerations add up. Right now it just sets the new speed disregarding the existing goal    
                .None(new Motion(newSpeed, RotationalSpeed.Zero)));
                
        Goal = newGoal;
    }

    public void TurnBy(Timed<RotationalSpeed> timedRotationalSpeedChange)
    {
        throw new NotImplementedException();
    }

    public void AccelerateTo(Timed<Speed> timedSpeed)
    {
        throw new NotImplementedException();
    }

    public void TurnAt(Timed<RotationalSpeed> timedRotationalSpeed)
    {
        throw new NotImplementedException();
    }
}