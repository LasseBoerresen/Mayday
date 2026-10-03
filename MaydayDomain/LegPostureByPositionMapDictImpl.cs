using Generic;
using RobotDomain.Geometry;
using RobotDomain.Time;
using UnitsNet;
using static Generic.UnitsNetExtensions;
using Length = UnitsNet.Length;

namespace MaydayDomain;

/// <summary>
/// Maps any 3d tip position to a set of leg postures
/// </summary>
public record LegPostureByPositionMapDictImpl : LegPostureByPositionMap
{
    public LegPostureByPositionMapDictImpl(IReadOnlyDictionary<Xyz, List<MaydayLegPosture>> Map)
    {
        this.Map = Map;
    }

    public class EmptyMapException : Exception;

    // TODO handle that loaded map could be different cellSize, which would mess everything up
    static readonly Length CellSize = Length.FromMeters(1.0 / 64.0); // binary number for 100% float accuracy

    public IReadOnlyDictionary<Xyz, List<MaydayLegPosture>> Map 
    { 
        get;
        init
        {
            if (value.Count == 0)
                throw new EmptyMapException();
                
            field = value;
        }
    }

    /// <summary>
    /// A map with a single mapping, from the origin to <see cref="MaydayLegPosture.Neutral"/>, for legs that
    /// never need real posture lookups, such as echo legs. Any other tip position falls back to the current posture.
    /// </summary>
    /// <remarks>An empty map is rejected with <see cref="EmptyMapException"/>, so it cannot stand in here.</remarks>
    public static LegPostureByPositionMap CreateNeutral() =>
        new LegPostureByPositionMapDictImpl(
            new Dictionary<Xyz, List<MaydayLegPosture>> { [Xyz.Zero] = [MaydayLegPosture.Neutral] });

    /// <Inheritdoc />
    public MaydayLegPosture GetFor(Xyz tipPosition, MaydayLegPosture currentPosture)
    {   
        // Find corresponding cell position for tipPosition and closes neighbour
        // and interpolate between them.
        
        var cellPosition = GetCellCenterPositionFor(tipPosition);
        var cellPositionNeighbor = GetNeighborCellCenterPositionFor(tipPosition);
        var fractionOfProgressBetweenCells = tipPosition.GetFractionOfProgressBetween(cellPosition, cellPositionNeighbor);
     
        var posture = GetClosestFor(cellPosition, currentPosture);
        var postureNeighbor = GetClosestFor(cellPositionNeighbor, currentPosture);
        var postureInterpolated = MaydayLegPosture.InterpolateBetween(posture, postureNeighbor, fractionOfProgressBetweenCells);

        return postureInterpolated;
    }

    MaydayLegPosture GetClosestFor(Xyz cellPosition, MaydayLegPosture posture)
    {
        // TODO return Result<MaydayLegPosture, NoPostureError> if not found
        //  and force caller to handle that not all input xyz are reachable.  
        var possiblePostures = Map
            .LookFor(cellPosition)
            .IfNone(() =>
            {
                // TODO write what the cellPosition the current posture has, to figure out how far off the goal is. 
                Console.WriteLine($"No leg posture for cell position {cellPosition}, using current");
                return [posture];
                // throw new InvalidOperationException($"No leg posture for cell position {cellPosition}");
            });
            
        return possiblePostures
            .OrderBy(p => p.DistanceTo(posture))
            .FirstOption()
            .IfNone(() => throw new NotSupportedException($"All 'some' cell position sets should be non-empty."));
        
        // return Map[cellPosition]
        //     .OrderBy(p => p.DistanceTo(posture))
        //     .First();
    }

    // TODO Why would a leg have more than two possible postures? It is
    //  basically only knee direction thaT matters... Find the two best postures, 
    //  which lies closest to the cell center, and throw away others. Maybe it
    //  is even a pair, with optional values, not a list of postures.  
    public static LegPostureByPositionMapDictImpl BuildNew()
    {
        var dict = BuildNewDictionary();
        
        return new LegPostureByPositionMapDictImpl(dict);
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
        MaydayLegFactory legFactory = new(new EchoJointFactory(), new EmptyPostureByPositionMapping());
       
        var leg = legFactory.CreateLeg(MaydayLegId.LeftBack);
        return leg;
    }

    static void EnsureCellDensity(Dictionary<Xyz, List<MaydayLegPosture>> map)
    {
        var minimumPosturesPerCell = 1;
        
        if (map.Any(kvp => kvp.Value.Count < minimumPosturesPerCell))
            throw new Exception($"Not enough postures for each cell, min: {minimumPosturesPerCell}.");
    }

    /// <summary>
    /// Find the single closest neighbouring cell position 
    /// </summary>
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

    class EmptyPostureByPositionMapping : LegPostureByPositionMap
    {
        public MaydayLegPosture GetFor(Xyz tipPosition, MaydayLegPosture currentPosture)
        {
            throw new NotSupportedException($"{nameof(EmptyPostureByPositionMapping)} cannot do mappings.");
        }
    }
}
