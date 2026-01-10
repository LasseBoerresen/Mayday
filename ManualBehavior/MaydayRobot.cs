using LanguageExt;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;

namespace ManualBehavior;

public class MaydayRobot(BehaviorController behaviorController, CancellationTokenSource cancelTokenSource)
{
    public static Eff<MaydayRobot> CreateWithTerminalPostureBehaviorController(TimeProvider timeProvider)
    {
        CancellationTokenSource cancellationTokenSource = new();

        return InstantPostureMaydayMotionPlanner
            .Create(cancellationTokenSource, timeProvider)
            .Map(mp => new TerminalPostureBehaviorController(mp, cancellationTokenSource, timeProvider))
            .Map(bc => new MaydayRobot(bc, cancellationTokenSource));
    }

    public static Eff<MaydayRobot> CreateWithBabyLegsBehaviorController(TimeProvider timeProvider)
    {
        CancellationTokenSource cancellationTokenSource = new();
        
        return StepByStepLearningInstantPostureMaydayMotionPlanner
            .Create(cancellationTokenSource, timeProvider)
            .Map(mp => new BabyLegsBehaviorController(mp, cancellationTokenSource, timeProvider))
            .Map(bc => new MaydayRobot(bc, cancellationTokenSource));
    }

    public static Eff<MaydayRobot> CreateWithSwayBehavior(TimeProvider timeProvider)
    {
        CancellationTokenSource cts = new();
        
        return InstantPostureMaydayMotionPlanner
            .Create(cts, timeProvider)
            .Map(mp => new SwayBehaviorController(mp, cts.Token, timeProvider))
            .Map(bc => new MaydayRobot(bc, cts));
    }

    public Unit Start() => behaviorController.Start();

    public void Stop() => cancelTokenSource.Cancel();
}
