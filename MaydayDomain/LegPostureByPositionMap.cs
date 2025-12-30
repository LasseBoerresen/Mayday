using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;
using Generic;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;
using static System.Math;
using Length = UnitsNet.Length;

namespace MaydayDomain;

public class LegPostureByPositionMap
{
    const string FilePath = "MaydayLegPostureMap.json";
    static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new LengthJsonConverter(),
            new AngleJsonConverter()
        }
    };
    static readonly IReadOnlyDictionary<Xyz, List<MaydayLegPosture>> Map;
    private static readonly Length CellSize = Length.FromMeters(0.01);

    static LegPostureByPositionMap()
    {
        var leg = CreateEchoLeg();

        Map = BuildDictionary(leg).ToFrozenDictionary();
    }

    static MaydayLeg CreateEchoLeg()
    {
        MaydayLegFactory legFactory = new(new EchoJointFactory());
       
        var leg = legFactory.CreateLeg(MaydayLegId.LeftBack);
        return leg;
    }

    static IDictionary<Xyz, IImmutableSet<MaydayLegPosture>> LoadFromFile()
    {
        var json = File.ReadAllText(FilePath);
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, List<MaydayLegPosture>>>(json, SerializerOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize file {FilePath}");

        return deserialized.ToDictionary(
            kvp => JsonSerializer.Deserialize<Xyz>(kvp.Key, SerializerOptions) 
                   ?? throw new InvalidOperationException($"Failed to deserialize Xyz key: {kvp.Key}"),
            kvp => (IImmutableSet<MaydayLegPosture>)kvp.Value.ToImmutableHashSet());
    }

    public static void StoreToFile(IReadOnlyDictionary<Xyz, List<MaydayLegPosture>> dict)
    {

        var oneMeterString =  JsonSerializer.Serialize(Xyz.One, SerializerOptions);
        var oneMeterLength =  JsonSerializer.Deserialize<Xyz>(oneMeterString, SerializerOptions);
            
        var serializable = dict
            .OrderBy(kvp => kvp.Key.X.Meters)
            .ThenBy(kvp => kvp.Key.Y.Meters)
            .ThenBy(kvp => kvp.Key.Z.Meters)
            .ToDictionary(
                kvp => JsonSerializer.Serialize(kvp.Key),
                kvp => kvp.Value);

        var json = JsonSerializer.Serialize(serializable, SerializerOptions);
        File.WriteAllText(FilePath, json);
    }

    public static IReadOnlyDictionary<Xyz, List<MaydayLegPosture>> BuildDictionary(MaydayLeg leg)
    {
        var angleStep = Angle.FromRevolutions(1.0 / 256);
        
        
        var map = new Dictionary<Xyz, List<MaydayLegPosture>>();

        MaydayLeg.ApplyForJointAngleRanges(AppendPosture, angleStep);

        EnsureCellDensity(map);

        return map;

        // local func
        void AppendPosture(MaydayLegPosture posture)
        {
            leg.SetPosture(posture);
                    
            var position = leg.GetTipPosition();
            var cellPosition = GetCellCenterPositionFor(position);

            map.AppendElement(key: cellPosition, element: posture);
        }
    }

    private static void EnsureCellDensity(Dictionary<Xyz, List<MaydayLegPosture>> map)
    {
        var minimumPosturesPerCell = 1;
        
        if (map.Any(kvp => kvp.Value.Count < minimumPosturesPerCell))
            throw new Exception($"Not enough postures for each cell, min: {minimumPosturesPerCell}.");
    }

    static Xyz GetNeighborCellCenterPositionFor(Xyz tipPosition)
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
        // Use decimals to avoid precision problems with floats
        var positionMeters = (decimal)position.Meters;
        var cellSizeMeters = (decimal)CellSize.Meters;
        
        // Offsetting so coordinates are not halfway between two cells.  
        var offsetPositionMeters = positionMeters + cellSizeMeters / (decimal)2.0;

        var residualMeters = Modulo(offsetPositionMeters, cellSizeMeters);

        var cellCenterCoordinate = Length.FromMeters((double)(offsetPositionMeters - residualMeters));
        return cellCenterCoordinate;
    }
    
    /// <Remarks>
    /// Remainder, i.e. '%' on floats gave precision problems, therefore calculating residual manually
    /// </Remarks>
    public static decimal Modulo(decimal a, decimal b)
    {
        return a - b * Floor(a / b);
    }

    /// <summary>
    /// Get leg joint posture which results in the given tip position 
    /// </summary>
    /// <returns>Posture for tip position closest to current posture</returns>
    /// <exception cref="InvalidOperationException">If the tip position is unreachable</exception>
    public static MaydayLegPosture GetFor(Xyz tipPosition, MaydayLegPosture currentPosture)
    {
        var cellPosition = GetCellCenterPositionFor(tipPosition);
        var neighborCellPosition = GetNeighborCellCenterPositionFor(tipPosition);
        var fractionOfProgressBetweenCells = tipPosition.GetFractionOfProgressBetween(cellPosition, neighborCellPosition);
        
        var posture = GetClosestFor(cellPosition, currentPosture);
        var neighborPosture = GetClosestFor(neighborCellPosition , currentPosture);
        var interpolatedPosture = MaydayLegPosture.InterpolateBetween(posture, neighborPosture, fractionOfProgressBetweenCells);

        return interpolatedPosture;
    }

    static MaydayLegPosture GetClosestFor(Xyz cellPosition, MaydayLegPosture posture)
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
}