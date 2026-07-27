using Dynamixel;
using RobotDomain.Motion;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;
using RotationDirection = RobotDomain.Structures.RotationDirection;

namespace Test.Unit.Dynamixel;

public class EchoJointDriver : JointDriver
{
    JointState InitialState { get; }
    readonly Dictionary<JointId, JointState> _states = new(); 

    public EchoJointDriver(JointState state) => InitialState = state;

    public void SetGoalAngleFor(JointId id, Timed<Angle> goalAngleTimed)
    {
        _states.TryAdd(id, InitialState);

        _states[id] = _states[id] with { AngleGoal = goalAngleTimed, Angle = goalAngleTimed.Target};
    }

    public void Initialize(JointId id, RotationDirection rotationDirection) {}

    public JointState GetState(JointId id)
    {
        _states.TryAdd(id, InitialState);
        
        return _states[id];
    }

    public void Dispose()
    {
       
    }
}
