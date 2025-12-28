using LanguageExt;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using Test.Unit;
using Test.Utilities;
using Xunit;
using static Test.Unit.TestObjectFactory;
using Length = UnitsNet.Length;

namespace Test.Integration.Main;

public class MaydayLegTests
{
    static readonly MaydayMotionPlanner MotionPlanner = InstantPostureMaydayMotionPlanner
        .Create(new CancellationTokenSource())
        .RunUnsafe();

    public static TheoryData<string, LinkName, Transform>
        DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected()
    {
        return TestObjectFactory.DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected();
    }

    /// <summary>
    /// Tests basic movements of legs, on real robot.
    /// </summary>
    [PhysicalRobotTheory]
    [MemberData(nameof(DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected))]
    void GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected(
        string testId, LinkName linkName, Transform expectedTransform)
    {
        // Given
        MotionPlanner.SetPosture(MaydayLegPosture.Neutral);

        // When
        var actualTransform = MotionPlanner.GetTransformsOf(linkName).LF;

        // Then
        AssertTransformEqual(testId, expectedTransform, actualTransform);
    }
    
    /// <summary>
    /// Tests basic inverse kinematics of legs, on real robot.
    /// </summary>
    [PhysicalRobotFact]
    void GivenLegsWithTipAtX015_WhenGetTipPosition_ThenReturnsX015()
    {
        // Given
        var minX = Length.FromMeters(-0.22);
        var maxX = Length.FromMeters(0.20);
        var deltaX = Length.FromMeters(0.015);
        
        for (var x = maxX; x > minX; x -= deltaX)
        {
            MotionPlanner.SetTipPositionsForLegs(MaydayStructureSet<Xyz>.FromSingle(
                new(Length.FromMeters(0.12), Length.Zero, x)));

            Thread.Sleep(TimeSpan.FromSeconds(0.1));    
        }
        
        for (var x = minX; x < maxX; x += deltaX)
        {
            MotionPlanner.SetTipPositionsForLegs(MaydayStructureSet<Xyz>.FromSingle(
                new(Length.FromMeters(0.12), Length.Zero, x)));

            Thread.Sleep(TimeSpan.FromSeconds(0.1));    
        }
        
        // When
        // var actualPosition = MotionPlanner.GetPositionsOf(LinkName.Tip).LF;

        // Then
        // AssertXyzEqual(testId: "bla", expectedPosition, actualPosition); 
    }
}
