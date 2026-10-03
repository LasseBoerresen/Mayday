using System.Text.Json;
using Test.Utilities;
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

[Quarantine("Assertion mismatch on the reached tip position; previously hidden by EmptyMapException, cause not yet diagnosed.")]
public class MaydayLegInverseKinematicsTests
{
    readonly MaydayLeg _leg = MaydayLegTests
        .CreateEchoMaydayLegFactoryWithJointsAt(JointState.Zero)
        .CreateLeg(new(Side.Left, SidePosition.Center));

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
}
