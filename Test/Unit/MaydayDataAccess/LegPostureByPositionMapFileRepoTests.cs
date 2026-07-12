using MaydayDataAccess;
using MaydayDomain;
using Xunit;
using Xunit.Abstractions;

namespace Test.Unit.MaydayDataAccess;

public class LegPostureByPositionMapFileRepoTests(ITestOutputHelper testOutputHelper)
{
    /// <summary>
    /// Not a test, but a builder of new <see cref="LegPostureByPositionMap"/>
    /// </summary>
    // [Fact(Skip = $"Run only to rebuild and store {nameof(LegPostureByPositionMap)}")]
    [Fact]
    public void RebuildDictLegPostureByPositionMap()
    {
        var newMap = LegPostureByPositionMap.BuildNew();

        Print(newMap);

        new LegPostureByPositionMapFileRepo().Store(newMap);
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
