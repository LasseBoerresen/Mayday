using mayday.mayday;

namespace mayday.Behavior;

public class StandBehaviorController(MaydayStructure structure) : BehaviorController
{
    public void Stand()
    {
        structure.Posture = MaydayPosture.Standing;
    }

    public void Sit()
    {
        throw new NotImplementedException();
    }
}
