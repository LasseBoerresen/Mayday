using Generic;
using MaydayDomain;
using MaydayDomain.Components;
using Moq;
using RobotDomain.Geometry;
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
        var mayStructure = MaydayStructure.CreateEcho();
        
        // When
        var coxaMotorTransforms = mayStructure.GetTransformsOf(LinkName.CoxaMotor);

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

    [Fact]
    public void GivenStructureWithStandingPosture__WhenGetCurrentLean__ThenIsZero()
    {
        // Given
        var mayStructure = MaydayStructure.CreateEcho();
        var standingPostureCommand = Timed<MaydayLegPosture>.Passed(MaydayLegPosture.Standing);
        mayStructure.SetPostureForAllLegs(standingPostureCommand);
        
        // When
        var actualLean = mayStructure.GetCurrentLean();
        
        // Then
        TestObjectFactory.AssertTransformEqual("testidfoo", Transform.Zero, actualLean);
    }
    
    [Fact]
    public void GivenStructureWithStandingPostureAndTipsMovedBackward1cm__WhenGetCurrentLean__ThenIs1cmForward()
    {
        // Given
        var xOffset = Length.FromCentimeters(1);
        
        var mayStructure = MaydayStructure.CreateEcho();
        
        var standingPostureCommand = Timed<MaydayLegPosture>.Passed(MaydayLegPosture.Standing);
        mayStructure.SetPostureForAllLegs(standingPostureCommand);
        
        var tipPositionsCenter = mayStructure.GetPositionsOf(LinkName.Tip);
        var tipPositionsBackward = tipPositionsCenter.Map(tp => tp with {X = tp.X - xOffset});
        mayStructure.MoveTipsTo(Timed<MaydayStructureSet<Xyz>>.Passed(tipPositionsBackward), CancellationToken.None);
        
        // When
        var actualLean = mayStructure.GetCurrentLean();
        
        // Then
        var forwardXyz1Cm = Xyz.Zero with {X = xOffset};
        var expectedLean = Transform.Zero with {Xyz = forwardXyz1Cm };

        TestObjectFactory.AssertTransformEqual("testidfoo", expectedLean, actualLean);
    }
}
