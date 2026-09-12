using AwesomeAssertions;
using MaydayDataAccess;
using MaydayDomain;
using RobotDomain.Geometry;
using Xunit;
using Xunit.Abstractions;

namespace Test.Unit.MaydayDataAccess;

public class LegPostureByPositionMapFileRepoTests(ITestOutputHelper testOutputHelper)
{
    static readonly DirectoryInfo TestDataDirInfo
        = new(Path.Combine(AppContext.BaseDirectory, "Unit", "MaydayDataAccess", "TestData"));

    [Fact]
    public void GivenMapFileWithOneMappingAndNeighbor__WhenLoad__ThenMapShouldUseThatMapping()
    {
        // Given
        FileInfo mapFileInfo = new(
            Path.Combine(TestDataDirInfo.FullName, "SingleLegPostureByPositionMap.json"));
        
        LegPostureByPositionMapFileRepo legPostureByPositionMapFileRepo = new(mapFileInfo);

        // When
        var map = legPostureByPositionMapFileRepo.Load();
        
        // Then
        var actualPosture = map.GetFor(new Xyz(0,0,0), MaydayLegPosture.Neutral);
        
        actualPosture.Should().Be(MaydayLegPosture.FromRevolutions(0.1, 0.2, 0.3));
    }

    [Fact]
    public void GivenEmptyMapFile__WhenLoad__ThenThrowsEmptyMapException()
    {
        // Given
        FileInfo mapFileInfo = new(
            Path.Combine(TestDataDirInfo.FullName, "EmptyLegPostureByPositionMap.json"));
        
        LegPostureByPositionMapFileRepo legPostureByPositionMapFileRepo = new(mapFileInfo);

        // When
        var loadAction = () => legPostureByPositionMapFileRepo.Load();
        
        // Then
        loadAction.Should().Throw<LegPostureByPositionMapDictImpl.EmptyMapException>();
    }

    /// <summary>
    /// Not a test, but a builder of new <see cref="LegPostureByPositionMap"/>
    /// </summary>
    // [Fact(Skip = $"Run only to rebuild and store {nameof(LegPostureByPositionMap)}")]
    [Fact]
    public void RebuildDictLegPostureByPositionMap()
    {
        var newMap = LegPostureByPositionMapDictImpl.BuildNew();

        Print(newMap);

        FileInfo mapFileInfo = new("MaydayLegPostureMap.json");
        LegPostureByPositionMapFileRepo legPostureByPositionMapFileRepo = new(mapFileInfo);
        
        legPostureByPositionMapFileRepo.Store(newMap);
    }

    void Print(LegPostureByPositionMapDictImpl map)
    {
        foreach (var keyValuePair in map.Map)
        {
            var posturesString = String.Join(",\n", keyValuePair.Value.Select(p => p.ToString()));
            testOutputHelper.WriteLine($"{keyValuePair.Key}: \n{posturesString}");
        }
    }
}
