using RobotDomain.Structures;

namespace Dynamixel;

public interface JointStateCache
{
    JointState GetFor(JointId id);
    void SetFor(JointId id, JointState state);
}
