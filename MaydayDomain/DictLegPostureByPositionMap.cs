using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;
using Generic;
using RobotDomain.Geometry;
using UnitsNet;
using Length = UnitsNet.Length;

namespace MaydayDomain;

public class DictLegPostureByPositionMap
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
    static readonly IReadOnlyDictionary<Xyz, IImmutableSet<MaydayLegPosture>> Map;

    static DictLegPostureByPositionMap()
    {
        Map = File.Exists(FilePath) 
            ? LoadFromFile().ToFrozenDictionary() 
            : new Dictionary<Xyz, IImmutableSet<MaydayLegPosture>>().ToFrozenDictionary();
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
        var angleStep = Angle.FromRevolutions(1.0/10);
        var cellSize = Length.FromMeters(0.005);
        
        var map = new Dictionary<Xyz, List<MaydayLegPosture>>();

        MaydayLeg.ApplyForJointAngleRanges(AppendPosture, angleStep);

        return map;

        // local func
        void AppendPosture(MaydayLegPosture posture)
        {
            leg.SetPosture(posture);
                    
            var position = leg.GetTipPosition();
            var cellPosition = GetCellPositionFor(position, cellSize);

            map.AppendElement(key: cellPosition, element: posture);
        }
    }

    static Xyz GetCellPositionFor(Xyz position, Length cellSize)
    {
        return new Xyz(
            GetCellCoordinate(position.X, cellSize), 
            GetCellCoordinate(position.Y, cellSize), 
            GetCellCoordinate(position.Z, cellSize));
    }

    static Length GetCellCoordinate(Length position, Length cellSize)
    {
        var residual = Length.FromMeters(position.Meters % cellSize.Meters);
        
        return position - residual + cellSize / 2.0;
    }

    /// <summary>
    /// Get leg joint posture which results in the given tip position 
    /// </summary>
    /// <returns>Posture for tip position closest to current posture</returns>
    /// <exception cref="InvalidOperationException">If the tip position is unreachable</exception>
    public static MaydayLegPosture GetFor(Xyz tipPosition, MaydayLegPosture currentPosture)
    {
        // TODO For higher precision, linear interpolation between two nearest
        //  cells could be implemented, which would be simpler than a
        //  minimization step.
        var possiblePostures = Map
            .LookFor(tipPosition)
            .IfNone(() => throw new InvalidOperationException($"No leg posture for tip position {tipPosition}"));
            
        return possiblePostures
            .OrderBy(p => p.DistanceTo(currentPosture))
            .FirstOption()
            .IfNone(() => throw new NotSupportedException($"All 'some' tip position sets should be non-empty."));
    }
}