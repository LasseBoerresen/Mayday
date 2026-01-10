using Generic;
using MaydayDomain;
using MaydayDomain.Components;
using Moq;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;
using Xunit;
using static MaydayDomain.MaydayLegId;

namespace Test.Unit.MaydayDomain;

public class MaydayStructureTests
{
    [Fact] 
    public void GivenSixUniqueLegs_WhenCreateMaydayRobot_ThenSucceeds()
    {
        // Given
        IList<Connection> connections = [];
        IList<Link> links = [];
        var thorax = Link.CreateThorax;
        
        Dictionary<MaydayLegId, MaydayLeg> legs = new()
        {
            { RightFront, new(connections, links) },
            { RightCenter, new(connections, links) },
            { RightBack, new(connections, links) },
            { LeftFront, new(connections, links) },
            { LeftCenter, new(connections, links) },
            { LeftBack, new(connections, links) }
        };

        // When
        MaydayStructure may = new(thorax, legs);

        // Then
        Assert.True(may != null);
    }

    [Fact] 
    public void GivenMaydayRobotWithMockLegs_WhenSetPosture_ThenCallsSetPostureOnAllLegs()
    {
        // Given
        var thorax = Link.CreateThorax;
        Dictionary<MaydayLegId, Mock<MaydayLeg>> mockLegsDict = new();
        AllLegIds.ToList().ForEach(id => mockLegsDict.Add(id, new(new List<Connection>(), new List<Link>())));
        var legsDict = mockLegsDict.MapValue(ml => ml.Object);
        
        MaydayStructure may = new(thorax, legsDict);
        
        // When
        var postureTimed = Timed<MaydayLegPosture>.Passed(MaydayLegPosture.Standing);
        may.SetPostureForAllLegs(postureTimed);

        // Then
        mockLegsDict.ToList().ForEach(kvp => 
            kvp.Value.Verify(l => l.SetPosture(postureTimed), Times.Once));
        
    }

    [Fact]
    public void GivenMaydayStructure_WhenGetTransformsOfCoxaMotors_ThenReturnsCorrectTransformsInThoraxFrame()
    {
        // Given
        EchoJointFactory echoJointFactory = new();
        MaydayStructure may = MaydayStructure.Create(echoJointFactory);
        
        // When
        var coxaMotorTransforms = may.GetTransformsOf(LinkName.CoxaMotor);

        // Then
        foreach (var legId in AllLegIds)
        {
            var actualTransform = coxaMotorTransforms.ToLegDict()[legId];
            var expectedTransform = Thorax.TransformFor(legId);

            Assert.True(
                actualTransform.IsAlmostEqual(expectedTransform, Length.FromMillimeters(0.1), Angle.FromDegrees(0.1)),
                $"Transform for {legId} does not match expected thorax transform." +
                $"\nActual: {actualTransform}" +
                $"\nExpected: {expectedTransform}");
        }
    }
}
