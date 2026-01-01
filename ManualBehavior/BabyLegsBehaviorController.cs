using LanguageExt;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Geometry;
using RobotDomain.Time;
using Length = UnitsNet.Length;

namespace ManualBehavior;

public class BabyLegsBehaviorController(
    MaydayMotionPlanner motionPlanner,
    CancellationTokenSource cancelTokenSource,
    TimeProvider timeProvider) : BehaviorController
{

    public Unit Start()
    {
        var initTimeStep = TimeSpan.FromSeconds(1.0);
        
        motionPlanner.SetPosture(timeProvider.ScheduleIn(MaydayLegPosture.Neutral, initTimeStep));
        Thread.Sleep(initTimeStep);
    
        var kickSize = Length.FromMeters(0.02);

        var timeStep = TimeSpan.FromSeconds(0.0);
        while (!cancelTokenSource.Token.IsCancellationRequested)
        {
            Kick(timeProvider.ScheduleIn(kickSize, timeStep));
            Thread.Sleep(timeStep);
        }
        
        return Unit.Default;
    }
    
    void Kick(Timed<Length> kickSizeTimed)
    {
        var tipDeltas = kickSizeTimed.Map(ks 
            => new MaydayStructureSet<Xyz>(
                Xyz.Random() * ks.Meters,
                Xyz.Random() * ks.Meters,
                Xyz.Random() * ks.Meters,
                Xyz.Random() * ks.Meters,
                Xyz.Random() * ks.Meters,
                Xyz.Random() * ks.Meters));
            
        motionPlanner.MoveTipPositions(tipDeltas);
    }
}