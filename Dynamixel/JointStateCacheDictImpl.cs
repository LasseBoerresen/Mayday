using System.Collections.Concurrent;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class JointStateCacheDictImpl : JointStateCache
{
    readonly ConcurrentDictionary<JointId, JointState> _cacheDict = new();

    public IEnumerable<JointId> GetIds() => _cacheDict.Keys;

    // TODO: Save timestamps for the values, and return extrapolated value
    //       based on time since recording. This means we can potentially
    //       sample much less often, by estimating the position. Effectively
    //       trading of accuracy for less congestion  
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
}
