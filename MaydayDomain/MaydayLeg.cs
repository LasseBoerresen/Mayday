using System.Collections.Immutable;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace MaydayDomain;

public class MaydayLeg
{
    public Pose CoxaPose => GetPoseOf(Coxa);
    public Pose FemurPose => GetPoseOf(Femur);
    public Pose TibiaPose => GetPoseOf(Tibia);
    public Pose TipPose => GetPoseOf(Tip);

    readonly ImmutableList<Link> _links;
    readonly IImmutableList<Connection> _connections;
    readonly IImmutableList<Joint> _joints;

    Link CoxaMotor  => _links[0];
    Link Coxa       => _links[1];
    Link FemurMotor => _links[2];
    Link Femur      => _links[3];
    Link TibiaMotor => _links[4];
    Link Tibia      => _links[5];
    Link Tip        => _links[6]; 
    
    public MaydayLeg(IList<Connection> connections, IList<Link> links)
    {
        _links = links.ToImmutableList();
        _connections = connections.ToImmutableList();
        _joints = connections.OfType<Joint>().ToImmutableList();
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

    Pose GetPoseOf(Link link) => CoxaMotor.GetPoseOf(link.Id);
}
