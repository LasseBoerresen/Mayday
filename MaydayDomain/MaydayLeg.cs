using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Generic;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace MaydayDomain;

public class MaydayLeg
{
    public static readonly Length Length = Length.FromMeters(0.26); 

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
            .ForEach(pair => pair.joint.SetAngleGoal(pair.angle));
    }

    IEnumerable<(Joint joint, Angle angle)> JointAndGoalAnglePairs(MaydayLegPosture posture)
    {
        return _joints.Zip(posture.AsListOfGoalAngles(), (joint, angle) => (joint, angle));
    }

    public Transform GetTransformOf(LinkName linkName) => GetTransformOf(LinkFromName(linkName));

    public Transform GetTransformOf(Link link) => CoxaMotor.GetTransformOf(link.Id);
    
    public Link LinkFromName(LinkName name)
    {
        return name switch
        {
            LinkName.CoxaMotor => CoxaMotor,
            LinkName.Coxa => Coxa,
            LinkName.FemurMotor => FemurMotor,
            LinkName.Femur => Femur,
            LinkName.TibiaMotor => TibiaMotor,
            LinkName.Tibia => Tibia,
            LinkName.Tip => Tip,
            _ => throw new NotSupportedException($"Link name '{name}' not supported")
        };
    }

    public Xyz GetTipPosition()
    {
        return GetTransformOf(LinkName.Tip).Xyz;
    }

    public void SetTipPositionTo(Xyz tipPosition)
    {
        var posture = LegPostureByPositionMap.GetFor(tipPosition, GetPosture());
        
        SetPosture(posture);
    }

    public void MoveTipPositionBy(Xyz tipOffset)
    {
        var posture = LegPostureByPositionMap.GetFor(GetTipPosition() + tipOffset, GetPosture());
        
        SetPosture(posture);
    }

    public void MoveTipPositionTo(Xyz tipPosition)
    {
        var posture = LegPostureByPositionMap.GetFor(tipPosition, GetPosture());
        
        SetPosture(posture);
    }

    public static void ApplyForJointAngleRanges(Action<MaydayLegPosture> action, Angle angleStep)
    {
        var limits = JointLimits.Defaults;
    
        for (var coxa = limits.CoxaMin; coxa < limits.CoxaMax; coxa += angleStep)
        for (var femur = limits.FemurMin; femur < limits.FemurMax; femur += angleStep)
        for (var tibia = limits.TibiaMin; tibia < limits.TibiaMax; tibia += angleStep)
            action(new MaydayLegPosture(coxa, femur, tibia));
    }
}
