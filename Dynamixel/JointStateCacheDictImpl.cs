using RobotDomain.Structures;

namespace Dynamixel;

public class JointStateCacheDictImpl : JointStateCache
{
    readonly IDictionary<JointId, JointState> _cacheDict = new Dictionary<JointId, JointState>();

    public JointState GetFor(JointId id) => _cacheDict[id];

    public void SetFor(JointId id, JointState state) => _cacheDict[id] = state;
}
