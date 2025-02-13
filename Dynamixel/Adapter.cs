using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public interface Adapter : IDisposable
{
    void SetGoal(JointId id, Angle angle);
    void Initialize(JointId id, RobotDomain.Structures.RotationDirection rotationDirection);
    JointState GetState(JointId id);
}