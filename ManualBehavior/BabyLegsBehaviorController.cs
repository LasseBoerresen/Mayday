using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;
using RobotDomain.Geometry;
using RobotDomain.Structures;
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
            var tipPositions = MotionPlanner
                .GetPositionsOf(LinkName.Tip)
                .Map(position => position + Xyz.Random() * kickSize.Meters);
                
            MotionPlanner.SetTipPositions(tipPositions);
        }
    }
}

