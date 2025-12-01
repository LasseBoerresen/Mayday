using System.Text.Json;
using FluentAssertions;
using MaydayDomain;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;
using static Test.Unit.TestObjectFactory;

namespace Test.Unit.MaydayDomain;

public class MaydayLegInverseKinematicsTests
{
    private readonly ITestOutputHelper _testOutputHelper;

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
        var expectedTipPosition = Xyz.Zero with { X = Length.FromMeters(0.1) };
        
        _leg.SetTipPositionTo(expectedTipPosition);
        
        // Then
        var actualTipPosition = _leg.GetTransformOf(LinkName.Tip).Xyz;
        
        AssertXyzEqual(testId, expectedTipPosition, actualTipPosition);
    }
    
    /// <summary>
    /// Not a test, but a builder of new <see cref="DictLegPostureByPositionMap"/>
    /// </summary>
    [Fact] //(Skip = $"Run only to rebuild and store {nameof(DictLegPostureByPositionMap)}")]
    public void RebuildDictLegPostureByPositionMap()
    {
        var newDict = DictLegPostureByPositionMap.BuildDictionary(_leg);

        foreach (var keyValuePair in newDict)
        {
            var posturesString = String.Join(",\n", keyValuePair.Value.Select(p => p.ToString()));
            _testOutputHelper.WriteLine($"{keyValuePair.Key}: \n{posturesString}");
        }
            

        // DictLegPostureByPositionMap.StoreToFile(newDict);
    }
    
}
