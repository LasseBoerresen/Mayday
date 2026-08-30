using EllieMain.Structures;
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
public class ArticulatedSteeringEllieMotionPlanner(
    EllieStructure structure,
    TimeProvider timeProvider) 
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
    bool isStarted = false;

    public void Start(CancellationToken ct)
    {
        // Avoid double starting. 
        if (isStarted)
             return;
        isStarted = true;    
        
        _trackingTask = PeriodicScheduler.RunAsync(
            action: ExecutePlanStep, 
            duration: Period, 
            ct);
    }
    
    /// <summary>
    /// To be used for tracking a single motion. 
    /// </summary>
    /// <param name="plan"></param>
    /// <exception cref="NotImplementedException"></exception>
    void ExecutePlanStep()
    {
        // If a plan exists, find current step
        
        Plan.IfSome(plan =>
        {
            var timedMotionStep = plan.At(timeProvider.GetUtcNow());

            var structureMotions = timedMotionStep.Map(MapBodyMotionToStructureMotions);
            
            structure.MoveAt(structureMotions);
        });
    }

    
    StructureSet<Speed, Angle> MapBodyMotionToStructureMotions(Motion bodyMotion)
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