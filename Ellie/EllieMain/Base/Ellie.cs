using LanguageExt;
using RobotDomain.Behavior;

namespace EllieMain.Base;

public class Ellie(BehaviorController behaviorController, CancellationTokenSource cancelTokenSource)
{
    public Unit Start() => behaviorController.Start();

    public void Stop() => cancelTokenSource.Cancel();
}

