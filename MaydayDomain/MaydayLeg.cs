using RobotDomain.Structures;

namespace MaydayDomain;

public class MaydayLeg(IEnumerable<Joint> joints)
{
    readonly IEnumerable<Joint> _joints = joints;

    public MaydayLegPosture GetPosture()
    {
        return new(_joints.Select(j => j.State.Angle));
    }

    public virtual void SetPosture(MaydayLegPosture posture)
    {
        _joints
            .Zip(posture.AsEnumerable(), (joint, angle) => (joint, angle) )
            .ToList()
            .ForEach(pair => pair.joint.SetAngleGoal(pair.angle));
    }

    public static MaydayLeg CreateLeg(MaydayLegId legId, JointFactory jointFactory)
    {
        var joints = Enumerable
            .Range(1, 3)
            .Select(i => new JointId(legId.Value + i))
            .Select(jointFactory.Create)
            .ToList();
            
        return new(joints);
    }

    public static IDictionary<MaydayLegId, MaydayLeg> CreateAll(JointFactory jointFactory)
    {
        return MaydayLegId
            .AllLegIds
            .Select(lId => new KeyValuePair<MaydayLegId, MaydayLeg>(lId, CreateLeg(lId, jointFactory)))
            .ToDictionary();
    }
}