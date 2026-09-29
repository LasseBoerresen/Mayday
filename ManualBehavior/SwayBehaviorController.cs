using LanguageExt;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Geometry;
using RobotDomain.Time;
using UnitsNet;
using Duration = UnitsNet.Duration;
using Length = UnitsNet.Length;

namespace ManualBehavior;

public class SwayBehaviorController(
    MaydayMotionPlanner motionPlanner,
    CancellationToken ct,
    TimeProvider timeProvider) 
    : BehaviorController
{
    readonly Duration TimeStep = Duration.FromSeconds(2.0);
    
    public Unit Start()
    {
        WakeUp();

        motionPlanner.Start(ct);
        
        PeriodicScheduler periodicScheduler = new(timeProvider);
        periodicScheduler.Run(SwayOnce, TimeStep , ct); 
        
        return Unit.Default;
    }

    void WakeUp()
    {
        // TODO change to setting a movement goal instead of manipulating
        //  joints directly. This is a better use of the robot structure
        //  abstraction  
        var timeStep = TimeSpan.FromSeconds(1.0);
        motionPlanner.SetPosture(timeProvider.ScheduleIn(MaydayLegPosture.Sitting, timeStep));
        Thread.Sleep(timeStep);
        
        motionPlanner.SetPosture(timeProvider.ScheduleIn(MaydayLegPosture.StandingWide, timeStep));
        Thread.Sleep(timeStep);
    }

    /// <summary>
    /// Sway halfway towards the central position plus randomly in any
    /// direction. This should exponentially keep the sway centered but also
    /// momentum-like, because the randomness is added around the last sway.
    /// </summary>
    void SwayOnce()
    {
        motionPlanner.Goal = CreateNewGoal();
    }

    Timed<Motion> CreateNewGoal()
    {
        var goalTimed = timeProvider.ScheduleIn(CreateGoalTowardsCenterWithSway(), TimeStep/10);
        
        Console.WriteLine("Goal lean: " + goalTimed.Target.Lean.Xyz);
        return goalTimed;
    }

    Motion CreateGoalTowardsCenterWithSway()
    {
        var previousGoal = GetPreviousGoal();
        var swayAmount = SwayAmount();
        
        // More erratic, because movement is only corrected towards center next
        // time, and corrections can add up with the random movement. 
        // var leanTowardsCenter = previousGoal.Lean.InDirectionTo(Motion.StandingStill.Lean, factor: 0.5);
        // var newGoal = previousGoal with { Lean = leanTowardsCenter + swayAmount };

        // More smooth, because random movement and gravity towards center is never in same direction. 
        var newRandomGoal = previousGoal with { Lean = previousGoal.Lean + swayAmount };
        var newGoal = newRandomGoal with { Lean= newRandomGoal.Lean.InDirectionTo(Motion.StandingStill.Lean, factor: 0.5)};
        
        return newGoal;
    }

    Motion GetPreviousGoal()
    {
        // If there somehow is no previous movement, simply set it to centered as a starting point.
        var previousGoal = motionPlanner.Goal
            .Map(timedMotion => timedMotion.Target)
            .IfNone(Motion.StandingStill);
    
        return previousGoal;
    }

    static Transform SwayAmount()
    {
        var maxTranslation = Length.FromMeters(0.07);
        var maxRotation = Angle.Zero; //.FromRevolutions(0.125); // TODO Just starting without rotation randomness to keep it simple. Maybe rotation is buggy. 
               
        var swayAmount = Transform.Random(maxTranslation, maxRotation);
        
        // Uncomment to just only having random Z movements or other. 
        // swayAmount = swayAmount with { Xyz = swayAmount.Xyz with { X = Length.Zero, Y = Length.Zero } };
        
        return swayAmount;
    }
}
