using Domain.Behavior;

namespace Domain;

public class Mayday(BehaviorController behaviorController)
{
    public void Start()
    {
        behaviorController.Start();
    }
}
