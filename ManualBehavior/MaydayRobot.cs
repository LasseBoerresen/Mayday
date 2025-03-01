using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;

namespace ManualBehavior;

public class MaydayRobot(BehaviorController behaviorController, CancellationTokenSource cancelTokenSource)
{
    public static MaydayRobot CreateWithTerminalPostureBehaviorController()
    {
        CancellationTokenSource cancellationTokenSource = new();
        MaydayMotionPlanner maydayMotionPlanner = InstantPostureMaydayMotionPlanner.Create(cancellationTokenSource);

        BehaviorController behaviorController = new TerminalPostureBehaviorController(
            maydayMotionPlanner, cancellationTokenSource);

        return new(behaviorController, cancellationTokenSource);
    }

    public static MaydayRobot CreateWithBabyLegsBehaviorController()
    {
        CancellationTokenSource cancellationTokenSource = new();
        var maydayMotionPlanner = StepByStepLearningInstantPostureMaydayMotionPlanner.Create(cancellationTokenSource);

        var behaviorController = new BabyLegsBehaviorController(maydayMotionPlanner, cancellationTokenSource);

        return new(behaviorController, cancellationTokenSource);
    }

    public void Start() => behaviorController.Start();

    public void Stop() => cancelTokenSource.Cancel();
}
