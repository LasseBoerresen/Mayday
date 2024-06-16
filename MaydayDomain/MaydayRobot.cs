using System.Collections.Immutable;

namespace MaydayDomain;

public class MaydayRobot
{
    private readonly ImmutableSortedDictionary<MaydayLegId, MaydayLeg> _legs;

    public MaydayRobot(IDictionary<MaydayLegId, MaydayLeg> legs)
    {
        _legs = legs.ToImmutableSortedDictionary();
    }

    public void SetPostureForAllLegs(MaydayLegPosture posture)
    {
        _legs.ToList().ForEach(kvp => kvp.Value.SetPosture(posture));
    }
}