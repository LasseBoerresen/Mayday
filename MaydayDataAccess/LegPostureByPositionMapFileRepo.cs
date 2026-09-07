using System.Collections.Frozen;
using System.Text.Json;
using Generic;
using MaydayDataAccess.Geometry;
using MaydayDomain;

namespace MaydayDataAccess;

public class LegPostureByPositionMapFileRepo(FileInfo mapFileInfo) : LegPostureByPositionMapRepo
{
    public LegPostureByPositionMap Load()
    {
        var json = File.ReadAllText(mapFileInfo.FullName);
        
        // Must deserialize key to string first, because json cannot have non-string keys.
        var deserializedStringKey = JsonSerializer.Deserialize<Dictionary<string, List<MaydayLegPostureDao>>>(json)
            ?? throw new InvalidOperationException($"Failed to deserialize file {mapFileInfo}");

        var deserializedXyzDaoKey = deserializedStringKey.ToDictionary(
            kvp => XyzDao.FromKey(kvp.Key), 
            kvp => kvp.Value);

        var mapDict = deserializedXyzDaoKey.ToFrozenDictionary(
            kvp => kvp.Key.ToDomain(),
            kvp => kvp.Value.Map(legPostureDao => legPostureDao.ToDomain()).ToList()); 

        return new LegPostureByPositionMap(mapDict);
    }

    public void Store(LegPostureByPositionMap map)
    {
        var serializable = map.Map
            .OrderBy(kvp => kvp.Key.X.Meters)
            .ThenBy(kvp => kvp.Key.Y.Meters)
            .ThenBy(kvp => kvp.Key.Z.Meters)
            .ToDictionary(
                kvp => XyzDao.FromDomain(kvp.Key).ToKey(),
                kvp => kvp.Value.Map(MaydayLegPostureDao.FromDomain));

        var json = JsonSerializer.Serialize(serializable, SerializerOptions);

        File.WriteAllText(mapFileInfo.FullName, json);
    }
    
    static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };
}