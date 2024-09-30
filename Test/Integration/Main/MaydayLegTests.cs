using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using Test.Unit;
using Xunit;
using static Test.Unit.TestObjectFactory;

namespace Test.Integration.Main;

public class MaydayLegTests
{
    static readonly MaydayMotionPlanner MotionPlanner = InstantPostureMaydayMotionPlanner.Create();

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
        MotionPlanner.SetPosture(MaydayLegPosture.Neutral);

        // When
        var actualTransform = MotionPlanner.GetTransformsOf(linkName).LF;

        // Then
        AssertTransformEqual(testId, expectedTransform, actualTransform);
    }
}
