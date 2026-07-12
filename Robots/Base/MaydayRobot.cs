using LanguageExt;
using RobotDomain.Behavior;

namespace Robots.Base;

public class MaydayRobot(BehaviorController behaviorController, CancellationTokenSource cancelTokenSource)
{
    public Unit Start() => behaviorController.Start();

    public void Stop() => cancelTokenSource.Cancel();
}
