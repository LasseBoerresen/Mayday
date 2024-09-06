using System.Collections.Immutable;
using Generic;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace MaydayDomain;

public class MaydayLeg
{
    readonly ImmutableList<Link> _links;
    readonly IImmutableList<Connection> _connections;
    readonly IImmutableList<Joint> _joints;

    public Link CoxaMotor  => _links[0];
    public Link Coxa       => _links[1];
    public Link FemurMotor => _links[2];
    public Link Femur      => _links[3];
    public Link TibiaMotor => _links[4];
    public Link Tibia      => _links[5];
    public Link Tip        => _links[6]; 
    
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
            .ForEach(pair => pair.joint.SetAngleGoal(pair.angle));
    }

    IEnumerable<(Joint joint, Angle angle)> JointAndGoalAnglePairs(MaydayLegPosture posture)
    {
        return _joints.Zip(posture.AsListOfGoalAngles(), (joint, angle) => (joint, angle));
    }

    public Transform GetTransformOf(Link link) => CoxaMotor.GetTransformOf(link.Id);
    
    public Link LinkFromName(MaydayLink name)
    {
        return name switch
        {
            MaydayLink.CoxaMotor => CoxaMotor,
            MaydayLink.Coxa => Coxa,
            MaydayLink.FemurMotor => FemurMotor,
            MaydayLink.Femur => Femur,
            MaydayLink.TibiaMotor => TibiaMotor,
            MaydayLink.Tibia => Tibia,
            MaydayLink.Tip => Tip,
            _ => throw new NotSupportedException($"Link name '{name}' not supported")
        };
    }
}
