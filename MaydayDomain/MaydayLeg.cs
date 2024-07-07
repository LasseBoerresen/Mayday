using System.Collections.Immutable;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace MaydayDomain;

public class MaydayLeg
{
    readonly IImmutableList<Joint> _joints;
    readonly ImmutableList<Link> _links;

    public MaydayLeg(IList<Joint> joints, IList<Link> links)
    {
        _joints = joints.ToImmutableList();
        _links = links.ToImmutableList();
    }

    public const int JointCount = 3;

    public MaydayLegPosture GetPosture()
    {
        return new(_joints.Select(j => j.State.Angle));
    }

    public virtual void SetPosture(MaydayLegPosture posture)
    {
        JointAndGoalAnglePairs(posture)
            .ToList()
            .ForEach(pair => pair.joint.SetAngleGoal(pair.angle));
    }

    IEnumerable<(Joint joint, Angle angle)> JointAndGoalAnglePairs(MaydayLegPosture posture)
    {
        return _joints.Zip(posture.AsListOfGoalAngles(), (joint, angle) => (joint, angle));
    }

    public static MaydayLeg CreateLeg(MaydayLegId legId, JointFactory jointFactory)
    {
        var links = CreateLinks();
        var joints = CreateLinkedJoints(legId, jointFactory, links);

        return new(joints, links);
    }

    static IList<Link> CreateLinks()
    {
        return [Link.CreateBase, Link.CreateCoxa, Link.CreateFemur, Link.CreateTibia];
    }

    static IList<Joint> CreateLinkedJoints(MaydayLegId legId, JointFactory jointFactory, IList<Link> links)
    {
        return JointId_And_ParentChildPairs(legId, links)
            .Select(pair => CreateLinkedJoint(jointFactory, pair.jointId, pair.parentAndChild))
            .ToList();
    }

    static IEnumerable<(JointId jointId, (Link parent, Link child) parentAndChild)> JointId_And_ParentChildPairs(
        MaydayLegId legId,
        IList<Link> links)
    {
        return GenerateJointIds(legId)
            .Zip(ParentChildPairs(links), (jointId, parentAndChild) => (jointId, parentAndChild));
    }

    static Joint CreateLinkedJoint(
        JointFactory jointFactory, 
        JointId jointId, 
        (Link parent, Link child) parentAndChild)
    {
        return jointFactory.Create(parentAndChild.parent, parentAndChild.child, jointId);
    }

    static IEnumerable<JointId> GenerateJointIds(MaydayLegId legId)
    {
        return Enumerable
            .Range(1, JointCount)
            .Select(i => new JointId(legId.Value() + i));
    }

    static IEnumerable<(Link parent, Link child)> ParentChildPairs(IList<Link> links)
    {
        var parents = links.SkipLast(1);
        var children = links.Skip(1);
        
        return parents.Zip(children, (parent, child) => (parent, child));
    }

    public static IDictionary<MaydayLegId, MaydayLeg> CreateAll(JointFactory jointFactory)
    {
        return MaydayLegId
            .AllLegIds
            .Select(lId => new KeyValuePair<MaydayLegId, MaydayLeg>(lId, CreateLeg(lId, jointFactory)))
            .ToDictionary();
    }

    public Pose GetOriginOfCoxaJoint()
    {
        return Pose.Zero;
    }

    public Pose GetOriginOfFemurJoint()
    {
        throw new NotImplementedException();
    }

    public Pose GetOriginOfTibiaJoint()
    {
        throw new NotImplementedException();
    }

    public Pose GetOriginTibiaTip()
    {
        throw new NotImplementedException();
    }
}
