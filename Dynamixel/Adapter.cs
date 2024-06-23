using RobotDomain.Structures;

namespace Dynamixel;

public interface Adapter
{
    void SetGoal(JointId id, PositionAngle angle);
}