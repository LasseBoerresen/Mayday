using System.Collections.Immutable;
using Generic;
using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain;

public class MaydayStructure
{
    readonly ImmutableSortedDictionary<MaydayLegId, MaydayLeg> _legs;

    public MaydayStructure(IDictionary<MaydayLegId, MaydayLeg> legs)
    {
        _legs = legs.ToImmutableSortedDictionary();
    }

    public MaydayStructureSet<Xyz> GetTipPoints()
    {
        return MaydayStructureSet<Xyz>.FromLegDict(_legs.MapValue(l => l.GetTransformOf(l.Tip).Xyz));
    }

    public void SetPosture(MaydayStructurePosture posture)
    {
        _legs.ForEach(kvp => kvp.Value.SetPosture(posture.ToLegDict()[kvp.Key]));
    }

    public MaydayStructureSet<MaydayLegPosture> GetPosture()
    {
        return MaydayStructureSet<MaydayLegPosture>
            .FromLegDict(_legs.MapValue(l => l.GetPosture()));
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