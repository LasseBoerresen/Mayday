using System.Collections.Immutable;
using Generic;
using MaydayDomain.MotionPlanning;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace MaydayDomain;

// TODO over time, the structure might hold lots of sensors and stuff, so
//  managing legs should maybe be delegated to a "Legs" type. 
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

    /// <summary>
    /// To lean the thorax, move all tips opposite direction. Rotational lean
    /// results in some translation of the tip around the thorax origo 
    /// </summary>
    public void MoveThoraxTo(Transform lean, Duration duration, CancellationToken ct)
    {
        var extraLeanRequired = lean - GetCurrentLean();

        // TODO the underlying dynamixel adapter benefits from updating all
        //  motors at the same time, but here we don't wanna know. We just set
        //  target angle goals, which return immediately, and the adapter should
        //  then run at an appropriate frequency to set new goals for all
        //  updated joints.   
        _legs.ForEach(legAndId => 
            MoveThoraxBy(extraLeanRequired, legAndId.Value));
    }

    static void MoveThoraxBy(Transform lean, MaydayLeg leg)
    {
        // Adding/subtracting two transforms effectively translates and rotates
        // the lhs which is exactly what is required for tip movement. 
        leg.MoveTipPositionBy((GetTransformOfTipFor(leg) - lean).Xyz);
    }

    public void MoveTipsTo(MaydayStructureSet<Xyz> tipPositions, Duration duration, CancellationToken ct)
    {
        _legs
            .Select(legAndId => legAndId.Value)
            .Zip(tipPositions)
            .ForEach(legAndTipPosition =>
                MoveTipTo(legAndTipPosition.Second, legAndTipPosition.First));
    }
    
    static void MoveTipTo(Xyz position, MaydayLeg leg)
    {
        // Adding/subtracting two transforms effectively translates and rotates
        // the lhs which is exactly what is required for tip movement. 
        leg.MoveTipPositionTo(position);
    }

    static Transform GetTransformOfTipFor(MaydayLeg leg)
    {
        throw new NotImplementedException(
            "TODO: get tip transform at thorax center, by transforming through "
            + "as of yet missing thorax link attached to dynamixel link");
    }

    Transform GetCurrentLean()
    {
        // TODO: To get the current lean, we need to find the ground plane from
        //  the lowest 3 feet on two sides and reverse calculate the lean. Or
        //  the orientation could come from an accelerometer, but the offset
        //  needs to come from the offset from the average foot position, 
        //  including rotation around z axis from the angle of the coxa joint.
        throw new NotImplementedException();
    }
}