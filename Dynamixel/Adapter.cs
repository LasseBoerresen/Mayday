using RobotDomain.Structures;

namespace Dynamixel;

public interface Adapter
{
    void SetGoal(JointId id, PositionAngle angle);
    void Initialize(JointId id);
    JointState GetState(JointId id);
}