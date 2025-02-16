using RobotDomain.Structures;

namespace Dynamixel;

public interface JointStateCache
{
    IEnumerable<JointId> GetIds();
    JointState GetFor(JointId id);
    void SetFor(JointId id, JointState state);
}
