using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace RobotDomain.Motion;


// TODO remove disposable, interfaces can never be disposable, because they do not deal with actual implementations 

/// <summary>
/// Controls multiple physical robot joints by their id
/// </summary>
public interface JointDriver : IDisposable
{
    void SetGoalAngleFor(JointId id, Timed<Angle> goalAngleTimed);
    void Initialize(JointId id, RotationDirection rotationDirection);
    JointState GetState(JointId id);
}