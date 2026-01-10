using System.Collections.Immutable;
using Generic;
using MaydayDomain.MotionPlanning;
using MaydayDomain.Components;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace MaydayDomain;

// TODO over time, the structure might hold lots of sensors and stuff, so
//  managing legs should maybe be delegated to a "Legs" type. 
public class MaydayStructure
{
    readonly Link _thorax;

    // TODO this should probably just be a structure set. More object oriented. 
    readonly ImmutableSortedDictionary<MaydayLegId, MaydayLeg> _legsById;

    public MaydayStructure(Link thorax, IDictionary<MaydayLegId, MaydayLeg> legs)
    {
        _thorax = thorax;
        _legsById = legs.ToImmutableSortedDictionary();
    }

    public MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName)
    {
        return _legsById
            .MapValue(l => GetTransformOf(linkName, l).Xyz)
            .ToMaydayStructureSet();
    }

    public Xyz GetPositionOf(LinkName linkName, MaydayLegId legId)
    {
        return GetTransformOf(linkName, _legsById[legId]).Xyz;
    }

    public MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName)
    {
        return _legsById
            .MapValue(l => GetTransformOf(linkName, l).Q)
            .ToMaydayStructureSet();
    }

    public MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName)
    {
        return _legsById
            .MapValue(l => GetTransformOf(linkName, l))
            .ToMaydayStructureSet();
    }

    Transform GetTransformOf(LinkName linkName, MaydayLeg leg)
    {
        return _thorax.GetTransformOf(leg.LinkFromName(linkName).Id);
    }

    public void SetPosture(Timed<MaydayStructurePosture> postureTimed)
    {
        _legsById.ForEach(kvp => kvp.Value.SetPosture(postureTimed.Map(p => p.ToLegDict()[kvp.Key])));
    }
    
    public void SetPosture(Timed<LegProperty<MaydayLegPosture>> legPropertyOfPostureTimed)
    {
        var leg = _legsById[legPropertyOfPostureTimed.Target.LegId];
        
        leg.SetPosture(legPropertyOfPostureTimed.Map(p => p.Value));
    }

    public MaydayStructureSet<MaydayLegPosture> GetPostures()
    {
        return _legsById
            .MapValue(l => l.GetPosture())
            .ToMaydayStructureSet();
    }

    public MaydayLegPosture GetPostureOf(MaydayLegId legId)
    {
        return _legsById[legId].GetPosture();
    }

    public void SetPostureForAllLegs(Timed<MaydayLegPosture> postureTimed)
    {
        _legsById.ForEach(kvp => kvp.Value.SetPosture(postureTimed));
    }
    
    public static MaydayStructure Create(JointFactory jointFactory)
    {
        var thorax = Link.CreateThorax;
        var legs = new MaydayLegFactory(jointFactory).CreateAll();

        legs.ForEach(kvp => 
            Attachment.NewBetween(thorax, kvp.Value.BaseLink, Thorax.TransformFor(kvp.Key)));

        return new(thorax, legs);
    }

    public static MaydayStructure CreateEcho()
    {
        EchoJointFactory echoJointFactory = new();
        
        return Create(echoJointFactory);
    }

    /// <summary>
    /// To lean the thorax, move all tips opposite direction. Rotational lean
    /// results in some translation of the tip around the thorax origo 
    /// </summary>
    public void MoveThoraxTo(Timed<Transform> leanTimed)
    {
        var extraLeanRequiredTimed = leanTimed.Map(l => l - GetCurrentLean());

        _legsById.ForEach(legAndId => 
            MoveThoraxBy(extraLeanRequiredTimed, legAndId.Value));
    }

    void MoveThoraxBy(Timed<Transform> leanTimed, MaydayLeg leg)
    {
        // Adding/subtracting two transforms effectively translates and rotates
        // the lhs which is exactly what is required for tip movement. 
        leg.MoveTipPositionBy(leanTimed.Map(lean => (GetTransformOfTipFor(leg) - lean).Xyz));
    }

    public void MoveTipsTo(Timed<MaydayStructureSet<Xyz>> tipPositionsTimed)
    {
        _legsById.Values
            .Zip(
                tipPositionsTimed.Map(tp => tp.AsEnumerable()).Sequence(), 
                (leg, tipPositionTimed) => (leg, tipPositionTimed))
            .ForEach(z => z.leg.MoveTipPositionTo(z.tipPositionTimed));
    }

    Transform GetTransformOfTipFor(MaydayLeg leg)
    {
        return GetTransformOf(LinkName.Tip, leg);
    }

    public Transform GetCurrentLean()
    {
        // TODO: To get the current lean, we need to find the ground plane from
        //  the lowest 3 feet on two sides and reverse calculate the lean. Later
        //  the orientation could come from an accelerometer, but the xy-offset
        //  needs to come from the offset from the average foot position, 
        //  including rotation around z axis from the angle of the coxa joint.
        throw new NotImplementedException();
    }
}