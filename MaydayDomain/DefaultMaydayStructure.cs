using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using Generic;
using MaydayDomain.MotionPlanning;
using MaydayDomain.Components;
using RobotDomain.Geometry;
using RobotDomain.Geometry.SystemNumerics;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace MaydayDomain;

// TODO over time, the structure might hold lots of sensors and stuff, so
//  managing legs should maybe be delegated to a "Legs" type. 

public class DefaultMaydayStructure : MaydayStructure
{
    readonly Xyz _thoraxOrigin = Xyz.Zero;
    readonly Link _thorax;
    readonly MaydayStructureSet<MaydayLeg> _legs;

    public DefaultMaydayStructure(Link thorax, IDictionary<MaydayLegId, MaydayLeg> legs)
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

    /// <summary>
    /// To lean the thorax, move all tips opposite direction. Rotational lean
    /// results in some translation of the tip around the thorax origo 
    /// </summary>
    public void MoveThoraxTo(Timed<Motion> motionTimed)
    {
        var verticalTipMovementTimed = motionTimed.Map(motion =>
            GetVerticalTipMovementWithGroundingAdjustment(motion.Lean));

        MoveTipsBy(verticalTipMovementTimed);

        MaydayStructureSet<Xyz> GetVerticalTipMovementWithGroundingAdjustment(Transform lean)
        {
            var currentLean = GetCurrentLean();
            var extraLeanRequired = lean - currentLean;
            var tipMovementRequired = -extraLeanRequired.Xyz;

            var legClearancesFromGround = GetLegClearancesFromGround();
        
            return legClearancesFromGround.Map(legClearance => 
                tipMovementRequired with { Z = tipMovementRequired.Z - legClearance});
        
            MaydayStructureSet<Length> GetLegClearancesFromGround()
            {
                var groundPlane = CalculateGroundPlane();

                return GetPositionsOf(LinkName.Tip)
                    .Map(tipPosition => new Ray3D(tipPosition, GravityDirection))
                    .Map(tipGravityRay => tipGravityRay.DistanceToPlane(groundPlane));
            }
        }
    }

    public void MoveTipsBy(Timed<MaydayStructureSet<Xyz>> offsetXyzsTimed)
    {
        var tipPositionsCurrent = GetPositionsOf(LinkName.Tip);
        
        var tipPositionsOffsetTimed = offsetXyzsTimed.Map(offsetXyzs => 
            offsetXyzs.CombineWith(tipPositionsCurrent, 
                combiner: (tipPosition, offsetXyz) => tipPosition + offsetXyz));
            
        MoveTipsTo(tipPositionsOffsetTimed);
    }
    
    public void MoveTipsBy(Timed<Xyz> offsetXyzTimed)
    {
        var tipPositionsCurrent = GetPositionsOf(LinkName.Tip);
        
        var tipPositionsOffsetTimed = 
            offsetXyzTimed.Map(offsetXyz => 
            tipPositionsCurrent.Map(tipPosition => 
                tipPosition + offsetXyz));
            
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
        // TODO: The xy-offset needs to come from the offset from the average
        //  foot position, including rotation around z axis from the angle of
        //  the coxa joint.

        var groundClearance = GetGroundClearance();
        var tipPositionsMean = GetPositionsOf(LinkName.Tip).Mean();
        
        var thoraxTranslation = new Xyz(
            -tipPositionsMean.X,
            -tipPositionsMean.Y, 
            groundClearance);

        // TODO: I can get the z rotation as the average coxa angle. Well, if
        //  the tips are at equal stances. 

        return new Transform(thoraxTranslation, ThoraxRotation);

        Length GetGroundClearance()
        {
            var groundPlane = CalculateGroundPlane();

            Length clearance = CenterOfGravityRay.DistanceToPlane(groundPlane);
            
            Debug.Assert(clearance > Length.FromMeters(0.0), $"Ground clearance is negative: {clearance}");
            Debug.Assert(clearance < Length.FromMeters(0.5), $"Ground clearance is unreasonable high: {clearance}");
            
            return clearance;
        }
    }
    
    Plane CalculateGroundPlane()
    {
        // Note: Coxa motor is the lowest wide point on the body that has a known position.
        var potentialGroundPoints = 
            GetPositionsOf(LinkName.Tip).Concat(
            GetPositionsOf(LinkName.FemurMotor));   
                
        var tenCentimeterSquaredArea = Area.FromSquareMeters(1e-2);
        var potentialGroundTriangles = potentialGroundPoints
            .Combinations(n: 3)
            .Map(triplet => Triangle3D.FromList([.. triplet]))
            .Where(triangle => !(triangle.Area() < tenCentimeterSquaredArea));
            
                
        // Find triangle intersected by centerOfGravityRay with largest distance.
        Ray3D centerOfGravityRay = new(_thoraxOrigin, GravityDirection);

        var lowestTriangleUnderCenterOfGravity = potentialGroundTriangles
            .Map(triangle => (triangle, intersection: triangle.LookForIntersectionWith(centerOfGravityRay)))
            .Where(triangleIntersection => triangleIntersection.intersection != null)
            .MinBy(triangleIntersection => (triangleIntersection.triangle.Center() - _thoraxOrigin).Z)
            .triangle;

        return lowestTriangleUnderCenterOfGravity.ToPlane();

    }

    Ray3D CenterOfGravityRay => new(_thoraxOrigin, GravityDirection);
    
    Xyz GravityDirection => new(0, 0, -1); // In absense of an accelerometer, this is the best we can do.

    Q ThoraxRotation => Q.Unit; // Should probably be drived from GravityDirection when accelerometer works. 
}