using JetBrains.Annotations;
using MaydayDomain;
using MaydayDomain.Internal;
using Moq;
using RobotDomain.Geometry;
using Xunit;

namespace Test.Unit.MaydayDomain;

[TestSubject(typeof(MirroredInYLegPostureByPositionMap))]
public class MirroredInYLegPostureByPositionMapTests
{
    [Fact]
    public void GivenMap_WhenGetForTipPosition_ThenReturnsMapsPostureForPositionMirroredInY()
    {
        // Given
        var currentPosture = MaydayLegPosture.SittingTall;
        var postureForMirroredPosition = MaydayLegPosture.Neutral;

        Mock<LegPostureByPositionMap> mapMock = new();
        mapMock
            .Setup(m => m.GetFor(new Xyz(0.2, -0.05, -0.1), currentPosture))
            .Returns(postureForMirroredPosition);

        MirroredInYLegPostureByPositionMap mirroredMap = new(mapMock.Object);

        // When
        var posture = mirroredMap.GetFor(new Xyz(0.2, 0.05, -0.1), currentPosture);

        // Then
        Assert.Equal(postureForMirroredPosition, posture);
    }
}
