using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public interface Adapter : IDisposable
{
    void SetGoalAngleFor(JointId id, Angle angle);
    void Initialize(JointId id, RobotDomain.Structures.RotationDirection rotationDirection);
    JointState GetState(JointId id);
}