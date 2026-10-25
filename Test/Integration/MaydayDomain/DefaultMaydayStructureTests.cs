using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;
using MaydayDomain;
using Test.Unit;
using UnitsNet;
using Xunit;
using MaydayTestObjectMother = Test.Integration.MaydayDomain.Base.TestObjectMother;

namespace Test.Integration.MaydayDomain;

/// <summary>
/// <see cref="DefaultMaydayStructure"/> with echo joints and the posture map the robot uses.
/// </summary>
public class DefaultMaydayStructureTests
{
    [Fact]
    public void GivenStructureWithStandingPostureAndTipsMovedBackward3cm__WhenGetCurrentLean__ThenIs3cmForward()
    {
        // Given
        var mayStructure = MaydayTestObjectMother.CreateEchoStructureWithDefaultMap();
        
        var standingPostureCommand = Timed<MaydayLegPosture>.Passed(MaydayLegPosture.StandingWide);
        mayStructure.SetPostureForAllLegs(standingPostureCommand);
        
        
        // The posture map places tips only to about 1 cm (MAY-74), so move well beyond that accuracy.
        var offsetX = Length.FromCentimeters(3);
        var offsetXyz = Xyz.Zero with { X = -offsetX };
        
        mayStructure.MoveTipsBy(Timed<Xyz>.Passed(offsetXyz));
        
        // When
        var actualLean = mayStructure.GetCurrentLean();
        
        // Then
        // The z value is unknown, so just use the actual z. 
        var forwardXyz3Cm = Xyz.Zero with {X = offsetX, Z = actualLean.Xyz.Z};
        
        TestObjectFactory.AssertXyzEqual("testidfoo", forwardXyz3Cm, actualLean.Xyz);
    }

    public static TheoryData<TestInput> DataFor_GivenSittingTall_WhenMoveAllTipsForward5cm_ThenTipMoves5cmForward()
    {
        return new()
        {
            new(Id: "LeftFront", MaydayLegId: MaydayLegId.LeftFront),
            new(Id: "LeftCenter", MaydayLegId: MaydayLegId.LeftCenter),
            new(Id: "LeftBack", MaydayLegId: MaydayLegId.LeftBack),
            new(Id: "RightFront", MaydayLegId: MaydayLegId.RightFront),
            new(Id: "RightCenter", MaydayLegId: MaydayLegId.RightCenter),
            new(Id: "RightBack", MaydayLegId: MaydayLegId.RightBack),
        };
    }

    /// <summary>
    /// The echo prediction of <see cref="MaydayStructureManualTests.GivenSittingTallRobot_WhenMoveAllTipsForward5cm_ThenAllTipsMoveForward"/>.
    /// </summary>
    [Theory]
    [MemberData(nameof(DataFor_GivenSittingTall_WhenMoveAllTipsForward5cm_ThenTipMoves5cmForward))]
    public void GivenSittingTall_WhenMoveAllTipsForward5cm_ThenTipMoves5cmForward(TestInput input)
    {
        // Given
        var mayStructure = MaydayTestObjectMother.CreateEchoStructureWithDefaultMap();
        mayStructure.SetPostureForAllLegs(Timed<MaydayLegPosture>.Passed(MaydayLegPosture.SittingTall));
        var tipPositionBefore = mayStructure.GetPositionsOf(LinkName.Tip).ToLegDict()[input.MaydayLegId];

        // When
        var forward5Cm = Xyz.Zero with { X = Length.FromCentimeters(5) };
        mayStructure.MoveTipsBy(Timed<Xyz>.Passed(forward5Cm));

        // Then
        var tipPositionAfter = mayStructure.GetPositionsOf(LinkName.Tip).ToLegDict()[input.MaydayLegId];

        // The posture map places tips only to about 1 cm (MAY-74), which the default tolerance allows.
        TestObjectFactory.AssertXyzEqual(input.Id.Value, forward5Cm, tipPositionAfter - tipPositionBefore);
    }

    public record TestInput(TestId Id, MaydayLegId MaydayLegId);
}
