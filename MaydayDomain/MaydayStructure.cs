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
    readonly MaydayStructureSet<MaydayLeg> _legs;

    public MaydayStructure(Link thorax, IDictionary<MaydayLegId, MaydayLeg> legs)
    {
        _thorax = thorax;
        _legs = MaydayStructureSet<MaydayLeg>.FromLegDict(legs);
    }

    public MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName)
    {
        return _legs.Map(l => GetTransformOf(linkName, l).Xyz);
    }

    public Xyz GetPositionOf(LinkName linkName, MaydayLegId legId)
    {
        return GetTransformOf(linkName, _legs[legId]).Xyz;
    }

    public MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName)
    {
        return _legs.Map(l => GetTransformOf(linkName, l).Q);
    }

    public MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName)
    {
        return _legs.Map(l => GetTransformOf(linkName, l));
    }

    Transform GetTransformOf(LinkName linkName, MaydayLeg leg)
    {
        return _thorax.GetTransformOf(leg.LinkFromName(linkName).Id);
    }

    public void SetPosture(Timed<MaydayStructurePosture> postureTimed)
    {
        _legs.Zip(postureTimed.Map(p => p.ToSet()).Sequence())
            .ForEach(lAndP => lAndP.First.SetPosture(lAndP.Second));
    }

    public MaydayStructureSet<MaydayLegPosture> GetPostures()
    {
        return _legs.Map(l => l.GetPosture());
    }

    public MaydayLegPosture GetPostureOf(MaydayLegId legId)
    {
        return _legs[legId].GetPosture();
    }

    public void SetPostureForAllLegs(Timed<MaydayLegPosture> postureTimed)
    {
        _legs.ForEach(l => l.SetPosture(postureTimed));
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
        var currentLean = GetCurrentLean();
        var extraLeanRequiredTimed = leanTimed.Map(lean => lean - currentLean);

        MoveTipsBy(extraLeanRequiredTimed.Map(tl => tl.Xyz));

        // _legs.ForEach(leg => 
        //     MoveThoraxBy(extraLeanRequiredTimed, leg));
    }

    void MoveThoraxBy(Timed<Transform> leanOffsetTimed, MaydayLeg leg)
    {
        var tipTransform = GetTransformOfTipFor(leg);
    
        // Adding/subtracting two transforms effectively translates and rotates
        // the rhs which is exactly what is required for tip movement. 
        
        // TODO: Test this, I am not sure we are subtracting the right thing
        //  or if we should transform back to leg frame of reference 
        var tipTargetTimed = leanOffsetTimed.Map(
            lean => tipTransform.Xyz - lean.Xyz); // TODO Does not rotate for now 
        
        leg.MoveTipPositionTo(tipTargetTimed);
    }

    public void MoveTipsBy(Timed<Xyz> offsetXyzTimed)
    {
        var tipPositionsCurrent = GetPositionsOf(LinkName.Tip);
        
        var tipPositionsOffsetTimed = offsetXyzTimed.Map(
            offsetXyz => tipPositionsCurrent.Map(tp => tp + offsetXyz));
            
        MoveTipsTo(tipPositionsOffsetTimed);
    }

    public void MoveTipsTo(Timed<MaydayStructureSet<Xyz>> tipPositionsTimed)
    {
        var legByIdAndTipPositionTimed = _legs.ToLegDict()
            .Zip(
                tipPositionsTimed.Sequence(), 
                (legById, tipPositionTimed) => (legById, tipPositionTimed));
                
        legByIdAndTipPositionTimed.ForEach(z => 
            z.legById.Value.MoveTipPositionTo(
                z.tipPositionTimed.Map(
                    tipPos =>  tipPos.ViewedFrom(z.legById.Key))));
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

        var tipPositionsMean = GetPositionsOf(LinkName.Tip).Mean();
        

        // TODO: I can get the z rotation as the average coxa angle. Well, if
        //  the tips are at equal stances. But really I should calculate the 
        //  ground plane, and the thorax's angle and translation to that and
        //  its origo. 

        return new Transform(-tipPositionsMean, Q.Unit);
    }
}