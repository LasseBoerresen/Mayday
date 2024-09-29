using ManualBehavior;
using MaydayDomain.MotionPlanning;
using RobotDomain.Behavior;

namespace Main;

public class MaydayRobot(BehaviorController behaviorController, CancellationTokenSource cancelTokenSource)
{
    public static MaydayRobot CreateWithTerminalPostureBehaviorController()
    {
        MaydayMotionPlanner maydayMotionPlanner = InstantPostureMaydayMotionPlanner.Create();
        CancellationTokenSource cts = new();

        BehaviorController behaviorController = new TerminalPostureBehaviorController(maydayMotionPlanner, cts);

        return new(behaviorController, cts);
    }

    public void Start() => behaviorController.Start();

    public void Stop() => cancelTokenSource.Cancel();
}
