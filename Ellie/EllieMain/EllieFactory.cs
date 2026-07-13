using EllieMain.Base;
using RobotDomain.Behavior;

namespace EllieMain;

public class EllieFactory(
        BehaviorController behaviorController, 
        CancellationTokenSource cts)
{
    public Ellie CreateDefault()
    {
        return new Ellie(behaviorController, cts);
    }
}
