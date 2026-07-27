using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace Dynamixel;


// TODO move out of Dynamixel component, this is a more general robot This is a joint controller
// TODO remove disposable, interfaces can never be disposable, because they do not deal with actual implementations 

public interface Adapter : IDisposable
{
    void SetGoalAngleFor(JointId id, Timed<Angle> goalAngleTimed);
    void Initialize(JointId id, RobotDomain.Structures.RotationDirection rotationDirection);
    JointState GetState(JointId id);
}