using JetBrains.Annotations;
using MaydayDomain;
using MaydayDomain.Components;
using RobotDomain.Geometry;
using Xunit;

namespace Test.Unit.Components;

[TestSubject(typeof(Thorax))]
public class ThoraxTests
{
    public static TheoryData<TestInput> DataFor_GivenXyz_WhenViewedFromLeg_ThenReturnsExpectedXyz()
    {
        return new()
        {
            #region 0. Thorax origin should be behind the leg
            // Directly behind center legs. Corner legs are yawed 45°, but their mount position lies at ~41.4°
            // from the thorax origin, so the origin appears slightly to the side.

            new(
                Id: "0.0", 
                MaydayLegId: MaydayLegId.LeftCenter, 
                XyzInThoraxFrame: Xyz.Zero, 
                XyzInLegFrameExpected: new Xyz(-0.1, 0, 0)),
                
            new(
                Id: "0.1", 
                MaydayLegId: MaydayLegId.RightCenter, 
                XyzInThoraxFrame: Xyz.Zero, 
                XyzInLegFrameExpected: new Xyz(-0.1, 0, 0)),
            
            new(
                Id: "0.2", 
                MaydayLegId: MaydayLegId.LeftFront, 
                XyzInThoraxFrame: Xyz.Zero, 
                XyzInLegFrameExpected: new Xyz(-0.1131, 0.0071, 0)),
                
            new(
                Id: "0.3", 
                MaydayLegId: MaydayLegId.LeftBack, 
                XyzInThoraxFrame: Xyz.Zero, 
                XyzInLegFrameExpected: new Xyz(-0.1131, -0.0071, 0)),
            
            new(
                Id: "0.4", 
                MaydayLegId: MaydayLegId.RightFront, 
                XyzInThoraxFrame: Xyz.Zero, 
                XyzInLegFrameExpected: new Xyz(-0.1131, -0.0071, 0)),
                
            new(
                Id: "0.5", 
                MaydayLegId: MaydayLegId.RightBack, 
                XyzInThoraxFrame: Xyz.Zero, 
                XyzInLegFrameExpected: new Xyz(-0.1131, 0.0071, 0)),
            
            #endregion


            #region 1. Exact leg transform should result in Xyz.Zero
            
            new(
                Id: "1.0", 
                MaydayLegId: MaydayLegId.LeftCenter, 
                XyzInThoraxFrame: Thorax.LeftCenterTransform.Xyz, 
                XyzInLegFrameExpected: Xyz.Zero),
                
            new(
                Id: "1.1", 
                MaydayLegId: MaydayLegId.LeftFront, 
                XyzInThoraxFrame: Thorax.LeftFrontTransform.Xyz, 
                XyzInLegFrameExpected: Xyz.Zero),
                
            new(
                Id: "1.2", 
                MaydayLegId: MaydayLegId.LeftBack, 
                XyzInThoraxFrame: Thorax.LeftBackTransform.Xyz, 
                XyzInLegFrameExpected: Xyz.Zero),
                
            new(
                Id: "1.3", 
                MaydayLegId: MaydayLegId.RightCenter, 
                XyzInThoraxFrame: Thorax.RightCenterTransform.Xyz, 
                XyzInLegFrameExpected: Xyz.Zero),
                
            new(
                Id: "1.4", 
                MaydayLegId: MaydayLegId.RightFront, 
                XyzInThoraxFrame: Thorax.RightFrontTransform.Xyz, 
                XyzInLegFrameExpected: Xyz.Zero),
                
            new(
                Id: "1.5", 
                MaydayLegId: MaydayLegId.RightBack, 
                XyzInThoraxFrame: Thorax.RightBackTransform.Xyz, 
                XyzInLegFrameExpected: Xyz.Zero),
                
            #endregion
        };
    }

    [Theory]
    [MemberData(nameof(DataFor_GivenXyz_WhenViewedFromLeg_ThenReturnsExpectedXyz))]
    public void GivenXyzInThoraxFrame_WhenViewedFromLeg_ThenReturnsExpectedXyz(TestInput input)
    {
        // When
        var actualXyz = input.XyzInThoraxFrame.ViewedFrom(input.MaydayLegId);
        
        // Then
        TestObjectFactory.AssertXyzEqual(input.Id, input.XyzInLegFrameExpected, actualXyz);
    }

    public record TestInput(
        TestId Id, 
        MaydayLegId MaydayLegId, 
        Xyz XyzInThoraxFrame,
        Xyz XyzInLegFrameExpected);
}