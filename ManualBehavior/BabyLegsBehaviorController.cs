using LanguageExt;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Geometry;
using Length = UnitsNet.Length;

namespace ManualBehavior;

public class BabyLegsBehaviorController(
    MaydayMotionPlanner MotionPlanner,
    CancellationTokenSource cancelTokenSource) : BehaviorController
{
    public Unit Start()
    {
        MotionPlanner.SetPosture(MaydayLegPosture.Neutral);
        Thread.Sleep(TimeSpan.FromSeconds(1.0));
    
        var kickSize = Length.FromMeters(0.02);

        while (!cancelTokenSource.Token.IsCancellationRequested)
        {
            Kick(kickSize);
            Thread.Sleep(TimeSpan.FromSeconds(0.0));
        }
        
        return Unit.Default;
    }
    
    private void Kick(Length kickSize)
    {
        var tipDeltas = new MaydayStructureSet<Xyz>(
            Xyz.Random() * kickSize.Meters,
            Xyz.Random() * kickSize.Meters,
            Xyz.Random() * kickSize.Meters,
            Xyz.Random() * kickSize.Meters,
            Xyz.Random() * kickSize.Meters,
            Xyz.Random() * kickSize.Meters);
            
        MotionPlanner.MoveTipPositions(tipDeltas);
    }
}

