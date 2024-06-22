namespace Dynamixel;

public interface Adapter
{
    void SetGoal(Id id, PositionAngle angle);
}