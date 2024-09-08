using System.Collections.Immutable;
using Generic;
using RobotDomain.Structures;
using static MaydayDomain.MaydayLegId;

namespace MaydayDomain;

public class MaydayStructure
{
    readonly ImmutableSortedDictionary<MaydayLegId, MaydayLeg> _legs;

    public MaydayStructure(IDictionary<MaydayLegId, MaydayLeg> legs)
    {
        _legs = legs.ToImmutableSortedDictionary();
    }
    
    public void SetPosture(MaydayStructurePosture posture)
    {
        _legs.ForEach(kvp => kvp.Value.SetPosture(posture.ToLegDict()[kvp.Key]));
    }

    public MaydayStructurePosture GetPosture()
    {
        return new MaydayStructurePosture(
            RF: _legs[RightFront].GetPosture(),
            RC: _legs[RightCenter].GetPosture(),
            RB: _legs[RightBack].GetPosture(),
            LF: _legs[LeftFront].GetPosture(),
            LC: _legs[LeftCenter].GetPosture(),
            LB: _legs[LeftBack].GetPosture());
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