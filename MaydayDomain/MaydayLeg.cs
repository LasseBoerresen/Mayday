using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Generic;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace MaydayDomain;

public class MaydayLeg
{
    public static readonly Length Length = Length.FromMeters(0.26); 

    readonly ImmutableList<Link> _links;
    readonly IImmutableList<Connection> _connections;
    readonly IImmutableList<Joint> _joints;
    readonly LegPostureByPositionMap _legPostureByPositionMap;

    public Link BaseLink => CoxaMotor;
    Link CoxaMotor  => _links[0];
    Link Coxa       => _links[1];
    Link FemurMotor => _links[2];
    Link Femur      => _links[3];
    Link TibiaMotor => _links[4];
    Link Tibia      => _links[5];
    Link Tip        => _links[6];

    public MaydayLeg(
        IList<Connection> connections, 
        IList<Link> links, 
        LegPostureByPositionMap legPostureByPositionMap)
    {
        _connections = connections.ToImmutableList();
        _links = links.ToImmutableList();
        _legPostureByPositionMap = legPostureByPositionMap;
        
        _joints = connections.OfType<Joint>().ToImmutableList();
    }

    public const int JointCount = 3;

    public MaydayLegPosture GetPosture()
    {
        MaydayLegPosture maydayLegPosture = new(_joints.Select(j => j.State.Angle));
        return maydayLegPosture;
    }

    public virtual void SetPosture(Timed<MaydayLegPosture> posture)
    {
        JointAndGoalAnglePairs(posture)
            .ForEach(pair => pair.joint.SetAngleGoal(pair.angleGoalTimed));
    }

    IEnumerable<(Joint joint, Timed<Angle> angleGoalTimed)> JointAndGoalAnglePairs(
        Timed<MaydayLegPosture> posture)
    {
        var timedTargetsForJoints = posture
            .Map(p => p.AsListOfGoalAngles().AsEnumerable())
            .Sequence();
        
        return _joints.Zip(timedTargetsForJoints);
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

    public void SetTipPositionTo(Timed<Xyz> tipPositionTimed)
    {
        var postureTimed = tipPositionTimed.Map(tipPosition => 
            _legPostureByPositionMap.GetFor(tipPosition, GetPosture()));
        
        SetPosture(postureTimed);
    }

    public void MoveTipPositionBy(Timed<Xyz> tipOffsetTimed)
    {
        var posture = tipOffsetTimed.Map(tipOffset => 
            _legPostureByPositionMap.GetFor(GetTipPosition() + tipOffset, GetPosture()));
        
        SetPosture(posture);
    }

    public void MoveTipPositionTo(Timed<Xyz> tipPositionTimed)
    {
        var currentPosture = GetPosture();
        var postureTimed = tipPositionTimed.Map(
            tp => _legPostureByPositionMap.GetFor(tp, currentPosture));
        
        SetPosture(postureTimed);
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
