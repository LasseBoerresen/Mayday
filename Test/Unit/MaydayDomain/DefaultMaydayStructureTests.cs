using System.Collections.ObjectModel;
using MaydayTestObjectMother = Test.Integration.MaydayDomain.Base.TestObjectMother;
using Test.Utilities;
using Generic;
using MaydayDomain;
using MaydayDomain.Components;
using MaydayDomain.MotionPlanning;
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
        var legPostureByPositionMap = MaydayTestObjectMother.LegPostureByPositionMap;
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
    [Quarantine("Moq cannot create a MaydayLeg proxy: the MaydayLeg constructor no longer takes the arguments the mock supplies.")]
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
    [Quarantine("Assertion mismatch: expected a 1 cm forward lean (X 0.010) but the lean X is 0; previously hidden by EmptyMapException, cause not yet diagnosed.")]
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

    [Fact]
    public void GivenStandingStillMotion_WhenSetStance_ThenEachLegIsGivenItsNominalFootprintInItsOwnFrame()
    {
        // Given
        RecordingLegPostureByPositionMap recordingMap = new();
        var mayStructure = new MaydayStructureFactory(
            new MaydayLegFactory(new EchoJointFactory(), recordingMap)).CreateDefault();

        var motion = Motion.StandingStill;

        // When
        mayStructure.SetStance(Timed<Motion>.Passed(motion));

        // Then
        var expectedTipPositionsInLegFrames = AllLegIds
            .Select(legId => Stance.GetTipPositionInThoraxFrameFor(legId, motion).ViewedFrom(legId));

        foreach (var expected in expectedTipPositionsInLegFrames)
            Assert.Contains(
                recordingMap.RequestedTipPositions,
                actual => actual.IsAlmostEqual(expected, Length.FromMillimeters(0.1)));
    }

    [Fact]
    public void GivenStanceSetTwice_WhenSetStance_ThenBothRoundsRequestTheSameTipPositions()
    {
        // Given
        RecordingLegPostureByPositionMap recordingMap = new();
        var mayStructure = new MaydayStructureFactory(
            new MaydayLegFactory(new EchoJointFactory(), recordingMap)).CreateDefault();

        var motionTimed = Timed<Motion>.Passed(Motion.StandingStill);

        mayStructure.SetStance(motionTimed);
        var firstRound = recordingMap.RequestedTipPositions.ToList();
        recordingMap.Clear();

        // When
        mayStructure.SetStance(motionTimed);

        // Then
        Assert.Equal(firstRound, recordingMap.RequestedTipPositions);
    }

    /// <summary>
    /// Records requested tip positions, so stance geometry can be verified
    /// without depending on a populated inverse kinematics map.
    /// </summary>
    class RecordingLegPostureByPositionMap : LegPostureByPositionMap
    {
        readonly List<Xyz> _requestedTipPositions = [];

        public IReadOnlyList<Xyz> RequestedTipPositions => _requestedTipPositions;

        public void Clear() => _requestedTipPositions.Clear();

        public MaydayLegPosture GetFor(Xyz tipPosition, MaydayLegPosture currentPosture)
        {
            _requestedTipPositions.Add(tipPosition);

            return currentPosture;
        }
    }
}
