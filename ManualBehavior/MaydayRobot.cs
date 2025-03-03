using LanguageExt;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;

namespace ManualBehavior;

public class MaydayRobot(BehaviorController behaviorController, CancellationTokenSource cancelTokenSource)
{
    public static Eff<MaydayRobot> CreateWithTerminalPostureBehaviorController()
    {
        CancellationTokenSource cancellationTokenSource = new();

        return InstantPostureMaydayMotionPlanner
            .Create(cancellationTokenSource)
            .Map(mp => new TerminalPostureBehaviorController(mp, cancellationTokenSource))
            .Map(bc => new MaydayRobot(bc, cancellationTokenSource));
    }

    public static Eff<MaydayRobot> CreateWithBabyLegsBehaviorController()
    {
        CancellationTokenSource cancellationTokenSource = new();
        
        return StepByStepLearningInstantPostureMaydayMotionPlanner
            .Create(cancellationTokenSource)
            .Map(mp => new BabyLegsBehaviorController(mp, cancellationTokenSource))
            .Map(bc => new MaydayRobot(bc, cancellationTokenSource));
    }

    public Unit Start() => behaviorController.Start();

    public void Stop() => cancelTokenSource.Cancel();
}
