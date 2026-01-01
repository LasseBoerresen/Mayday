using System.Collections.Concurrent;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace Dynamixel;

public class JointStateCacheDictImpl : JointStateCache
{
    readonly ConcurrentDictionary<JointId, JointState> _cacheDict = new();

    public IEnumerable<JointId> GetIds() => _cacheDict.Keys;
    public IReadOnlyDictionary<JointId, JointState> GetById() => _cacheDict.AsReadOnly();

    public JointState GetFor(JointId id) => _cacheDict[id];

    public void SetFor(JointId id, JointState state) => _cacheDict[id] = state;

    public void SetAngleFor(JointId id, Angle angle)
    {
        // Must use update method, to ensure operation is atomic. 
        _cacheDict.AddOrUpdate(
            key: id, 
            addValueFactory: key => throw new NotSupportedException($"Error trying to set angle for uninitialized jointId: {id}"),
            updateValueFactory: (key, oldState) => oldState with { Angle = angle });
    }

    public void SetAngleGoalFor(JointId id, Timed<Angle> angleGoal)
    {
        // Must use update method, to ensure operation is atomic. 
        _cacheDict.AddOrUpdate(
            key: id,
            addValueFactory: key => throw new NotSupportedException($"Error trying to set angle goal for uninitialized jointId: {id}"),
            updateValueFactory: (key, oldState) => oldState with { AngleGoal = angleGoal });
    }
}
