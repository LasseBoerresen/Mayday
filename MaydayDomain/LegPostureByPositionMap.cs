using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;
using Generic;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;
using static System.Math;
using static Generic.UnitsNetExtensions;
using Length = UnitsNet.Length;

namespace MaydayDomain;

public record LegPostureByPositionMap(IReadOnlyDictionary<Xyz, List<MaydayLegPosture>> Map)
{
    static readonly Length CellSize = Length.FromMeters(1.0 / 64.0); // binary number for 100% float accuracy
    public static LegPostureByPositionMap CreateEmpty() => new(new Dictionary<Xyz, List<MaydayLegPosture>>());

    /// <summary>
    /// Get leg joint posture which results in the given tip position 
    /// </summary>
    /// <returns>Posture for tip position closest to current posture</returns>
    /// <exception cref="InvalidOperationException">If the tip position is unreachable</exception>
    public MaydayLegPosture GetFor(Xyz tipPosition, MaydayLegPosture currentPosture)
    {   
        var cellPosition = GetCellCenterPositionFor(tipPosition);
        var cellPositionNeighbor = GetNeighborCellCenterPositionFor(tipPosition);
        var fractionOfProgressBetweenCells = tipPosition.GetFractionOfProgressBetween(cellPosition, cellPositionNeighbor);
     
        var posture = GetClosestFor(cellPosition, currentPosture);
        var postureNeighbor = GetClosestFor(cellPositionNeighbor , currentPosture);
        var postureInterpolated = MaydayLegPosture.InterpolateBetween(posture, postureNeighbor, fractionOfProgressBetweenCells);

        return postureInterpolated;
    }

    MaydayLegPosture GetClosestFor(Xyz cellPosition, MaydayLegPosture posture)
    {
        var possiblePostures = Map
            .LookFor(cellPosition)
            .IfNone(() => throw new InvalidOperationException($"No leg posture for cell position {cellPosition}"));
            
        return possiblePostures
            .OrderBy(p => p.DistanceTo(posture))
            .FirstOption()
            .IfNone(() => throw new NotSupportedException($"All 'some' cell position sets should be non-empty."));
        
        // return Map[cellPosition]
        //     .OrderBy(p => p.DistanceTo(posture))
        //     .First();
    }

    public static LegPostureByPositionMap BuildNew()
    {
        var dict = BuildNewDictionary();
        
        return new LegPostureByPositionMap(dict);
    }
    
    static IReadOnlyDictionary<Xyz, List<MaydayLegPosture>> BuildNewDictionary()    
    {
        var leg = CreateEchoLeg();
    
        var angleStep = Angle.FromRevolutions(1.0 / 64);
        
        
        var map = new Dictionary<Xyz, List<MaydayLegPosture>>();

        MaydayLeg.ApplyForJointAngleRanges(AppendPosture, angleStep);

        // EnsureCellDensity(map);

        return map;

        // local func
        void AppendPosture(MaydayLegPosture posture)
        {
            leg.SetPosture(Timed<MaydayLegPosture>.Passed(posture));
                    
            var position = leg.GetTipPosition();
            var cellPosition = GetCellCenterPositionFor(position);

            map.AppendElement(key: cellPosition, element: posture);
        }
    }

    static MaydayLeg CreateEchoLeg()
    {
        MaydayLegFactory legFactory = new(new EchoJointFactory(), CreateEmpty());
       
        var leg = legFactory.CreateLeg(MaydayLegId.LeftBack);
        return leg;
    }

    static void EnsureCellDensity(Dictionary<Xyz, List<MaydayLegPosture>> map)
    {
        var minimumPosturesPerCell = 1;
        
        if (map.Any(kvp => kvp.Value.Count < minimumPosturesPerCell))
            throw new Exception($"Not enough postures for each cell, min: {minimumPosturesPerCell}.");
    }

    static Xyz GetNeighborCellCenterPositionFor(Xyz tipPosition)
    {
        var centerCellPos = GetCellCenterPositionFor(tipPosition);

        List<Length> offsets = [-CellSize, Length.FromMeters(0.0), CellSize];
        
        Dictionary<Xyz, Length> distancesByPositions = new(); 
        
        foreach (var x in offsets)
        foreach (var y in offsets)
        foreach (var z in offsets)
            AddDistanceFor(new Xyz(x, y, z));

        var distancesByPositionsOrdered = distancesByPositions.OrderBy(kvp => kvp.Value);
        var neighborCellCenterPosition = distancesByPositionsOrdered.First().Key;
        
        return neighborCellCenterPosition;

        void AddDistanceFor(Xyz xyzOffset)
        {
            if (xyzOffset == Xyz.Zero) 
                return;
            
            var xyz = centerCellPos + xyzOffset;
            var distance = tipPosition.DistanceToLineSegmentBetween(centerCellPos, xyz);
           
            distancesByPositions[xyz] = distance;
        }
    }
    
    static Xyz GetNeighborCellCenterPositionForFast(Xyz tipPosition)
    {
        var centerCellPos = GetCellCenterPositionFor(tipPosition);

        List<Length> offsets = [-CellSize, Length.FromMeters(0.0), CellSize];
        
        Xyz? closestCellPos = null;
        var closestDistance = Length.FromMeters(double.MaxValue);
        
        foreach (var x in offsets)
        foreach (var y in offsets)
        foreach (var z in offsets)
        {
            var xyz = centerCellPos + new Xyz(x, y, z);
            var distance = tipPosition.DistanceToLineSegmentBetween(centerCellPos, xyz);

            if (distance >= closestDistance || xyz == centerCellPos) 
                continue;
            
            closestCellPos = xyz;
            closestDistance = distance;
        }
        
        return closestCellPos ?? throw new InvalidOperationException("No cell found.");
        
    }
    
    static Xyz GetCellCenterPositionFor(Xyz position)
    {
        return new Xyz(
            GetCellCenterCoordinate(position.X), 
            GetCellCenterCoordinate(position.Y), 
            GetCellCenterCoordinate(position.Z));
    }

    static Length GetCellCenterCoordinate(Length position)
    {
        // Offsetting so coordinates are not halfway between two cells.  
        var offsetPosition = position + CellSize / 2.0;

        var residual = Modulo(offsetPosition, CellSize);

        var cellCenterCoordinate = offsetPosition - residual;
        return cellCenterCoordinate;
    }
}
