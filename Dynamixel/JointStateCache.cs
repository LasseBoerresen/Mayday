using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public interface JointStateCache
{
    IEnumerable<JointId> GetIds();
    JointState GetFor(JointId id);
    void SetFor(JointId id, JointState state);
    void SetAngleFor(JointId id, Angle angle);
}
