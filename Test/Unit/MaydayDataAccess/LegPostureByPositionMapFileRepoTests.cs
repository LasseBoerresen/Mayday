using AwesomeAssertions;
using MaydayDataAccess;
using MaydayDomain;
using Xunit;
using Xunit.Abstractions;

namespace Test.Unit.MaydayDataAccess;

public class LegPostureByPositionMapFileRepoTests(ITestOutputHelper testOutputHelper)
{
    static readonly DirectoryInfo TestDataDirInfo
        = new(Path.Combine(AppContext.BaseDirectory, "Unit", "MaydayDataAccess", "TestData"));

    [Fact]
    public void GivenEmptyMapFile__WhenLoad__ThenThrowsBadMapResolutionException()
    {
        // Given
        FileInfo mapFileInfo = new(
            Path.Combine(TestDataDirInfo.FullName, "EmptyLegPostureByPositionMap.json"));
        
        LegPostureByPositionMapFileRepo legPostureByPositionMapFileRepo = new(mapFileInfo);

        // When
        var loadAction = () => legPostureByPositionMapFileRepo.Load();
        
        // Then
        loadAction.Should().Throw<LegPostureByPositionMap.BadMapResolutionException>();
        
    }

    /// <summary>
    /// Not a test, but a builder of new <see cref="LegPostureByPositionMap"/>
    /// </summary>
    // [Fact(Skip = $"Run only to rebuild and store {nameof(LegPostureByPositionMap)}")]
    [Fact]
    public void RebuildDictLegPostureByPositionMap()
    {
        var newMap = LegPostureByPositionMap.BuildNew();

        Print(newMap);

        FileInfo mapFileInfo = new("MaydayLegPostureMap.json");
        LegPostureByPositionMapFileRepo legPostureByPositionMapFileRepo = new(mapFileInfo);
        
        legPostureByPositionMapFileRepo.Store(newMap);
    }

    void Print(LegPostureByPositionMap map)
    {
        foreach (var keyValuePair in map.Map)
        {
            var posturesString = String.Join(",\n", keyValuePair.Value.Select(p => p.ToString()));
            testOutputHelper.WriteLine($"{keyValuePair.Key}: \n{posturesString}");
        }
    }
}
