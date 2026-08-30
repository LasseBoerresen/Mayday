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
    readonly Duration TimeStep = Duration.FromSeconds(5.0);
    
    public Unit Start()
    {
        WakeUp();

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
        
        motionPlanner.SetPosture(timeProvider.ScheduleIn(MaydayLegPosture.Standing, timeStep));
        Thread.Sleep(timeStep);

        motionPlanner.Start(ct);
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
        var goalTimed = timeProvider.ScheduleIn(CreateGoalTowardsCenterWithSway(), TimeStep);
        
        Console.WriteLine("Goal lean: " + goalTimed.Target.Lean.Xyz);
        return goalTimed;
    }

    Motion CreateGoalTowardsCenterWithSway()
    {
        var previousGoal = GetPreviousGoal();
        
        var leanHalfwayToCenter = previousGoal.Lean.HalfWayTo(Motion.StandingStill.Lean);
        var swayAmount = SwayAmount();

        var newGoal = previousGoal with { Lean = leanHalfwayToCenter + swayAmount };
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
        var maxTranslation = Length.FromMeters(0.04);
        var maxRotation = Angle.FromRevolutions(0.125);
               
        var swayAmount = Transform.Random(maxTranslation, maxRotation);
        
        return swayAmount;
    }
}
