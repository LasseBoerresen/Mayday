using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;
using Generic;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;
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
    private static readonly Length CellSize = Length.FromMeters(0.005);

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

        return map;

        // local func
        void AppendPosture(MaydayLegPosture posture)
        {
            leg.SetPosture(posture);
                    
            var position = leg.GetTipPosition();
            var cellPosition = GetCellPositionFor(position);

            map.AppendElement(key: cellPosition, element: posture);
        }
    }

    static Xyz GetCellPositionFor(Xyz position)
    {
        return new Xyz(
            GetCellCoordinate(position.X), 
            GetCellCoordinate(position.Y), 
            GetCellCoordinate(position.Z));
    }

    static Length GetCellCoordinate(Length position)
    {
        var residual = Length.FromMeters(position.Meters % CellSize.Meters);
        
        return position - residual + CellSize / 2.0;
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
        var cellPosition = GetCellPositionFor(tipPosition);
        var possiblePostures = Map
            .LookFor(cellPosition)
            .IfNone(() => throw new InvalidOperationException($"No leg posture for tip position {tipPosition}"));
            
        return possiblePostures
            .OrderBy(p => p.DistanceTo(currentPosture))
            .FirstOption()
            .IfNone(() => throw new NotSupportedException($"All 'some' tip position sets should be non-empty."));
    }
}