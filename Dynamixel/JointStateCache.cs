using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace Dynamixel;

public interface JointStateCache
{
    IEnumerable<JointId> GetIds();
    IReadOnlyDictionary<JointId, JointState> GetById();
    JointState GetFor(JointId id);
    void SetFor(JointId id, JointState state);
    void SetAngleFor(JointId id, Angle angle);
    void SetAngleGoalFor(JointId id, Timed<Angle> angle);
}
