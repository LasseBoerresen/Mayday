using System.Collections.Frozen;
using System.Text.Json;
using Generic;
using MaydayDomain;
using RobotDomain.Geometry;

namespace MaydayDataAccess;

public class LegPostureByPositionMapFileRepo : LegPostureByPositionMapRepo
{
    const string FilePath = "MaydayLegPostureMap.json";

    public LegPostureByPositionMap Load()
    {
        var json = File.ReadAllText(FilePath);
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, List<MaydayLegPosture>>>(json, SerializerOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize file {FilePath}");

        return new LegPostureByPositionMap(
            deserialized.ToFrozenDictionary(
                kvp => JsonSerializer.Deserialize<Xyz>(kvp.Key, SerializerOptions) 
                    ?? throw new InvalidOperationException($"Failed to deserialize Xyz key: {kvp.Key}"),
                kvp => kvp.Value));
    }

    public void Store(LegPostureByPositionMap map)
    {
        var oneMeterString =  JsonSerializer.Serialize(Xyz.One, SerializerOptions);
        var oneMeterLength =  JsonSerializer.Deserialize<Xyz>(oneMeterString, SerializerOptions);
            
        var serializable = map.Map
            .OrderBy(kvp => kvp.Key.X.Meters)
            .ThenBy(kvp => kvp.Key.Y.Meters)
            .ThenBy(kvp => kvp.Key.Z.Meters)
            .ToDictionary(
                kvp => JsonSerializer.Serialize(kvp.Key),
                kvp => kvp.Value);

        var json = JsonSerializer.Serialize(serializable, SerializerOptions);
        File.WriteAllText(FilePath, json);
    }
    
    static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new LengthJsonConverter(),
            new AngleJsonConverter()
        }
    };
}
