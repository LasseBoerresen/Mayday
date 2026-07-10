using System.Collections.ObjectModel;
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

public class DefaultMaydayStructureTests
{
    [Fact] 
    public void GivenSixUniqueLegs_WhenCreateMaydayRobot_ThenSucceeds()
    {
        // Given
        IList<Connection> connections = [];
        IList<Link> links = [];
        var legPostureByPositionMap = LegPostureByPositionMap.CreateEmpty();
        var thorax = Link.CreateThorax;

        Dictionary<MaydayLegId, MaydayLeg> legs = new()
        {
            { RightFront, new(connections, links, legPostureByPositionMap) },
            { RightCenter, new(connections, links, legPostureByPositionMap) },
            { RightBack, new(connections, links, legPostureByPositionMap) },
            { LeftFront, new(connections, links, legPostureByPositionMap) },
            { LeftCenter, new(connections, links, legPostureByPositionMap) },
            { LeftBack, new(connections, links, legPostureByPositionMap) }
        };

        // When
        DefaultMaydayStructure may = new(thorax, legs);

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
        
        DefaultMaydayStructure may = new(thorax, legsDict);
        
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
        var mayStructure = MaydayStructureFactory.CreateEcho();
        
        // When
        var coxaMotorTransforms = mayStructure.GetTransformsOf(LinkName.CoxaMotor);

        // Then
        foreach (var legId in AllLegIds)
        {
            var actualTransform = coxaMotorTransforms.ToLegDict()[legId];
            var expectedTransform = Thorax.TransformFor(legId);

            TestObjectFactory.AssertTransformEqual("testidfoo", expectedTransform, actualTransform);
        }
    }

    [Fact]
    public void GivenStructureWithStandingPosture__WhenGetCurrentLean__ThenIsZero()
    {
        // Given
        var mayStructure = MaydayStructureFactory.CreateEcho();
        
        var standingPostureCommand = Timed<MaydayLegPosture>.Passed(MaydayLegPosture.StandingWide);
        mayStructure.SetPostureForAllLegs(standingPostureCommand);
        
        // When
        var actualLean = mayStructure.GetCurrentLean();
        
        // Then
        // The z value is unknown, so just use the actual z.
        var expectedLeanXyz = Xyz.Zero with {Z = actualLean.Xyz.Z};
        
        TestObjectFactory.AssertXyzEqual("testidfoo", expectedLeanXyz, actualLean.Xyz);
    }
    
    [Fact]
    public void GivenStructureWithStandingPostureAndTipsMovedBackward1cm__WhenGetCurrentLean__ThenIs1cmForward()
    {
        // Given
        var mayStructure = MaydayStructureFactory.CreateEcho();
        
        var standingPostureCommand = Timed<MaydayLegPosture>.Passed(MaydayLegPosture.StandingWide);
        mayStructure.SetPostureForAllLegs(standingPostureCommand);
        
        
        var offsetX = Length.FromCentimeters(1);
        var offsetXyz = Xyz.Zero with { X = -offsetX };
        
        mayStructure.MoveTipsBy(Timed<Xyz>.Passed(offsetXyz));
        
        // When
        var actualLean = mayStructure.GetCurrentLean();
        
        // Then
        // The z value is unknown, so just use the actual z. 
        var forwardXyz1Cm = Xyz.Zero with {X = offsetX, Z = actualLean.Xyz.Z};
        
        TestObjectFactory.AssertXyzEqual("testidfoo", forwardXyz1Cm, actualLean.Xyz);
    }
    
    // TODO: Test GetCurrentLean.Z by SetTipPositionsTo() in a circle, but with known Z. 
}
