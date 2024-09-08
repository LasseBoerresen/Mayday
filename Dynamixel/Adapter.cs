using RobotDomain.Structures;

namespace Dynamixel;

public interface Adapter
{
    void SetGoal(JointId id, StepAngle angle);
    void Initialize(JointId id);
    JointState GetState(JointId id);
}