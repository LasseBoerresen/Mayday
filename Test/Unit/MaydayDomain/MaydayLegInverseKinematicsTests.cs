using FluentAssertions;
using MaydayDomain;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;
using static Test.Unit.TestObjectFactory;

namespace Test.Unit.MaydayDomain;

public class MaydayLegInverseKinematicsTests
{
    readonly MaydayLeg _leg = MaydayLegTests
        .CreateMaydayLegFactoryWithJointsAt(JointState.Zero)
        .CreateLeg(new(Side.Left, SidePosition.Center));

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
}
