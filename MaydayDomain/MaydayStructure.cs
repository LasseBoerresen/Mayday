using System.Collections.Immutable;
using Generic;
using MaydayDomain.MotionPlanning;
using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain;

public class MaydayStructure
{
    // TODO this should probably just be a structure set. More object oriented. 
    readonly ImmutableSortedDictionary<MaydayLegId, MaydayLeg> _legs;

    public MaydayStructure(IDictionary<MaydayLegId, MaydayLeg> legs)
    {
        _legs = legs.ToImmutableSortedDictionary();
    }

    public MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName)
    {
        return _legs
            .MapValue(l => l.GetTransformOf(linkName).Xyz)
            .ToMaydayStructureSet();
    }

    public Xyz GetPositionOf(LinkName linkName, MaydayLegId legId)
    {
        return _legs[legId].GetTransformOf(linkName).Xyz;
    }

    public MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName)
    {
        return _legs
            .MapValue(l => l.GetTransformOf(linkName).Q)
            .ToMaydayStructureSet();
    }

    public MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName)
    {
        return _legs
            .MapValue(l => l.GetTransformOf(linkName))
            .ToMaydayStructureSet();
    }

    public void SetPosture(MaydayStructurePosture posture)
    {
        _legs.ForEach(kvp => kvp.Value.SetPosture(posture.ToLegDict()[kvp.Key]));
    }
    
    public void SetPosture(LegProperty<MaydayLegPosture> posture)
    {
        _legs[posture.LegId].SetPosture(posture.Value);
    }

    public MaydayStructureSet<MaydayLegPosture> GetPostures()
    {
        return _legs
            .MapValue(l => l.GetPosture())
            .ToMaydayStructureSet();
    }

    public MaydayLegPosture GetPostureOf(MaydayLegId legId)
    {
        return _legs[legId].GetPosture();
    }

    public void SetPostureForAllLegs(MaydayLegPosture posture)
    {
        _legs.ForEach(kvp => kvp.Value.SetPosture(posture));
    }

    public static MaydayStructure Create(JointFactory jointFactory)
    {
        var legs = new MaydayLegFactory(jointFactory).CreateAll();
        
        return new(legs);
    }
}