using System.Text.Json;
using FluentAssertions;
using MaydayDataAccess;
using MaydayDomain;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;
using static Test.Unit.TestObjectFactory;

namespace Test.Unit.MaydayDomain;

public class MaydayLegInverseKinematicsTests
{
    readonly ITestOutputHelper _testOutputHelper;

    readonly MaydayLeg _leg = MaydayLegTests
        .CreateEchoMaydayLegFactoryWithJointsAt(JointState.Zero)
        .CreateLeg(new(Side.Left, SidePosition.Center));

    public MaydayLegInverseKinematicsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void WhenSetReachableTipPositionTo_TheReachesThatPosition()
    {
        var testId = nameof(WhenSetReachableTipPositionTo_TheReachesThatPosition);
    
        // When
        var expectedTipPosition = Xyz.Zero with { X = Length.FromMeters(0.2) };
        var expectedTipPositionTimed = Timed<Xyz>.Passed(expectedTipPosition);
        
        _leg.SetTipPositionTo(expectedTipPositionTimed);
        
        // Then
        var actualTipPosition = _leg.GetTransformOf(LinkName.Tip).Xyz;
        
        AssertXyzEqual(testId, expectedTipPosition, actualTipPosition);
    }
    
    /// <summary>
    /// Not a test, but a builder of new <see cref="LegPostureByPositionMap"/>
    /// </summary>
    [Fact(Skip = $"Run only to rebuild and store {nameof(LegPostureByPositionMap)}")]
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
            _testOutputHelper.WriteLine($"{keyValuePair.Key}: \n{posturesString}");
        }
    }
}
