using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using Test.Unit;
using Xunit;

namespace Test.Integration.Main;

public class MaydayLegTests
{
    public static TheoryData<string, LinkName, Transform>
        DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected()
    {
        return TestObjectFactory.DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected();
    }

    [Theory]
    [MemberData(nameof(DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected))]
    void GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected(
        string testId, LinkName linkName, Transform expectedTransform)
    {
        // Given
        MaydayMotionPlanner motionPlanner = InstantPostureMaydayMotionPlanner.Create();
        motionPlanner.SetPosture(MaydayLegPosture.Neutral);

        // When
        var actualTransform = motionPlanner.GetTransformsOf(linkName).LF;

        // Then
        AssertTransformEqual(testId, expectedTransform, actualTransform);
    }
}
