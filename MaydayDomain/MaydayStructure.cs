using System.Collections.Immutable;
using RobotDomain.Structures;

namespace MaydayDomain;

public class MaydayStructure
{
    readonly ImmutableSortedDictionary<MaydayLegId, MaydayLeg> _legs;

    public MaydayStructure(IDictionary<MaydayLegId, MaydayLeg> legs)
    {
        _legs = legs.ToImmutableSortedDictionary();
    }

    public void SetPostureForAllLegs(MaydayLegPosture posture)
    {
        _legs.ToList().ForEach(kvp => kvp.Value.SetPosture(posture));
    }

    public static MaydayStructure Create(JointFactory jointFactory)
    {
        var legs = MaydayLeg.CreateAll(jointFactory);
        return new(legs);
    }
}