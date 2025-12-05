using LanguageExt;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Geometry;
using RobotDomain.Time;
using UnitsNet;
using Length = UnitsNet.Length;

namespace ManualBehavior;

public class SwayBehaviorController(
    MaydayMotionPlanner MotionPlanner,
    PeriodicScheduler PeriodicScheduler,
    CancellationToken ct) 
    : BehaviorController
{
    public Unit Start()
    {
        WakeUp();

        PeriodicScheduler.Run(SwayOnce, ct);
        
        return Unit.Default;
    }

    void WakeUp()
    {
        // TODO change to setting a movement goal instead of manipulating
        //  joints directly. This is a better use of the robot structure
        //  abstraction  
        MotionPlanner.SetPosture(MaydayLegPosture.Sitting);
        Thread.Sleep(TimeSpan.FromSeconds(1.0));
        
        MotionPlanner.SetPosture(MaydayLegPosture.Standing);
        Thread.Sleep(TimeSpan.FromSeconds(1.0));
    }

    Movement CenteredMovement => Movement.Zero(PeriodicScheduler.CurrentTimeStamp);

    /// <summary>
    /// Sway halfway towards the central position plus randomly in any
    /// direction. This should exponentially keep the sway centered but also
    /// momentum-like, because the randomness is added around the last sway.
    /// </summary>
    void SwayOnce()
    {
        MotionPlanner.SetGoal(CreateNewGoal());
    }

    Movement GetPreviousGoal()
    {
        // If there somehow is no previous movement, simply set it to centered as a starting point.
        var previousGoal = MotionPlanner.GetGoal()
            .IfNone(CenteredMovement);
    
        return previousGoal;
    }

    static Transform SwayAmount()
    {
        var maxTranslation = Length.FromMeters(0.05);
        var maxRotation = Angle.FromRevolutions(0.125);
               
        var swayAmount = Transform.Random(maxTranslation, maxRotation);
        
        return swayAmount;
    }

    Movement CreateNewGoal()
    {
        Movement previousGoal = GetPreviousGoal();
        
        return previousGoal with
        {
            Lean = previousGoal.Lean.HalfWayTo(CenteredMovement.Lean) + SwayAmount(),
            Duration = PeriodicScheduler.Duration,
            TimeStamp = PeriodicScheduler.CurrentTimeStamp
        };
    }
}
