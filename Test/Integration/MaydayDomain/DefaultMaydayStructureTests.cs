using RobotDomain.Geometry;
using RobotDomain.Time;
using MaydayDomain;
using Test.Unit;
using Test.Utilities;
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
    [Quarantine("Assertion mismatch: moving tips 3 cm backward gives a lean X of 0.007 instead of 0.030, because right legs reach the sideways mirror of their target with the left-leg posture map (MAY-75).")]
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
}
