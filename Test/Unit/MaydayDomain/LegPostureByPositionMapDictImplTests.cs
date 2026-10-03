using AwesomeAssertions;
using JetBrains.Annotations;
using MaydayDomain;
using RobotDomain.Geometry;
using Xunit;

namespace Test.Unit.MaydayDomain;

[TestSubject(typeof(LegPostureByPositionMapDictImpl))]
public class LegPostureByPositionMapDictImplTests
{
    [Fact]
    public void GivenNeutralMap_WhenGetForOrigin_ThenReturnsNeutralPosture()
    {
        // Given
        var map = LegPostureByPositionMapDictImpl.CreateNeutral();

        // When
        var posture = map.GetFor(Xyz.Zero, MaydayLegPosture.NeutralWithStraightFemur);

        // Then
        posture.Should().Be(MaydayLegPosture.Neutral);
    }

    [Fact]
    public void GivenNeutralMap_WhenGetForUnmappedPosition_ThenFallsBackToCurrentPosture()
    {
        // Given
        var map = LegPostureByPositionMapDictImpl.CreateNeutral();
        var currentPosture = MaydayLegPosture.FromRevolutions(0.1, 0.2, 0.3);

        // When
        var posture = map.GetFor(new Xyz(1, 1, 1), currentPosture);

        // Then
        posture.Should().Be(currentPosture);
    }

    [Fact]
    public void GivenEchoLegFactory_WhenCreateEchoStructure_ThenSucceeds()
    {
        // When
        var create = () => MaydayStructureFactory.CreateEcho();

        // Then
        create.Should().NotThrow();
    }
}
