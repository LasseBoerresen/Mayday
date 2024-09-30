using Dynamixel;
using RobotDomain.Structures;
using UnitsNet;

namespace Test.Unit;

public class EchoAdapter : Adapter
{
    JointState InitialState { get; }
    readonly Dictionary<JointId,JointState> _states = new(); 

    public EchoAdapter(JointState state) => InitialState = state;

    public void SetGoal(JointId id, Angle angle)
    {
        _states.TryAdd(id, InitialState);

        _states[id] = _states[id] with { Angle = angle };
    }

    public void Initialize(JointId id) {}

    public JointState GetState(JointId id)
    {
        _states.TryAdd(id, InitialState);
        
        return _states[id];
    }

    public void Dispose()
    {
       
    }
}
