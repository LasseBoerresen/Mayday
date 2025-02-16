using System.Collections.Concurrent;
using RobotDomain.Structures;

namespace Dynamixel;

public class JointStateCacheDictImpl : JointStateCache
{
    readonly IDictionary<JointId, JointState> _cacheDict = new ConcurrentDictionary<JointId, JointState>();

    public IEnumerable<JointId> GetIds() => _cacheDict.Keys;

    public JointState GetFor(JointId id) => _cacheDict[id];

    public void SetFor(JointId id, JointState state) => _cacheDict[id] = state;
}
