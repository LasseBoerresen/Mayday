using RobotDomain.Behavior;

namespace RobotDomain;

public class Mayday(BehaviorController behaviorController)
{
    public void Start()
    {
        behaviorController.Start();
    }
}
