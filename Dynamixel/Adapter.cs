using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace Dynamixel;

public interface Adapter : IDisposable
{
    void SetGoalAngleFor(JointId id, Timed<Angle> goalAngleTimed);
    void Initialize(JointId id, RobotDomain.Structures.RotationDirection rotationDirection);
    JointState GetState(JointId id);
}