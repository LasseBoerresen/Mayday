using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Geometry;
using UnitsNet;

namespace ManualBehavior;

public class BabyLegsBehaviorController(
    MaydayMotionPlanner MotionPlanner,
    CancellationTokenSource cancelTokenSource) : BehaviorController
{
    public void Start()
    {
        var kickSize = Length.FromMeters(0.01);

        while (!cancelTokenSource.Token.IsCancellationRequested)
        {
            Kick(kickSize);
            Thread.Sleep(TimeSpan.FromSeconds(1.0));
        }
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

