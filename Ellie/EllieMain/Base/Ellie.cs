using LanguageExt;
using RobotDomain.Behavior;

// TODO move out of base, ellie is not something every type in this project can depend on. 
namespace EllieMain.Base;

public class Ellie(BehaviorController behaviorController, CancellationTokenSource cancelTokenSource)
{
    public Unit Start() => behaviorController.Start();

    public void Stop() => cancelTokenSource.Cancel();
}

