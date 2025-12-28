using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public interface JointStateCache
{
    IEnumerable<JointId> GetIds();
    IReadOnlyDictionary<JointId, JointState> GetById();
    JointState GetFor(JointId id);
    void SetFor(JointId id, JointState state);
    void SetAngleFor(JointId id, Angle angle);
    void SetAngleGoalFor(JointId id, Angle angle);
    void SetAnglesFor(IDictionary<JointId, Angle> anglesById);
}
