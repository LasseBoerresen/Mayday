using ManualBehavior;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;

namespace Main;

public class MaydayRobot(BehaviorController behaviorController, CancellationTokenSource cancelTokenSource)
{
    public static MaydayRobot CreateWithTerminalPostureBehaviorController()
    {
        MaydayMotionPlanner maydayMotionPlanner = InstantPostureMaydayMotionPlanner.Create();
        CancellationTokenSource cancellationTokenSource = new();

        BehaviorController behaviorController = new TerminalPostureBehaviorController(
            maydayMotionPlanner, cancellationTokenSource);

        return new(behaviorController, cancellationTokenSource);
    }

    public static MaydayRobot CreateWithBabyLegsBehaviorController()
    {
        var maydayMotionPlanner = StepByStepLearningInstantPostureMaydayMotionPlanner.Create();
        CancellationTokenSource cancellationTokenSource = new();

        var behaviorController = new BabyLegsBehaviorController(maydayMotionPlanner, cancellationTokenSource);

        return new(behaviorController, cancellationTokenSource);
    }

    public void Start() => behaviorController.Start();

    public void Stop() => cancelTokenSource.Cancel();
}
