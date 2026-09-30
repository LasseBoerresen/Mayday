using MaydayDomain;
using MaydayDomain.Components;
using MaydayDomain.MotionPlanning;
using RobotDomain.Geometry;
using UnitsNet;
using Xunit;
using static MaydayDomain.MaydayLegId;

namespace Test.Unit.MaydayDomain.MotionPlanning;

public class StanceTests
{
    static readonly Length StanceRadius = Length.FromMeters(0.15);
    static readonly Length GroundClearance = Length.FromMeters(0.1);

    // Pure geometry, so no need for the loose tolerance used for actuated poses.
    static readonly Length Precision = Length.FromMillimeters(0.1);

    static Motion MotionWith(Transform lean) =>
        new(Twist.Zero, lean, StanceRadius);

    static Motion StandingStill =>
        MotionWith(Transform.FromXyz(Xyz.Zero with { Z = GroundClearance }));

    public static TheoryData<LegInput> DataForEachLeg =>
        new()
        {
            new(Id: "LeftFront", LegId: LeftFront),
            new(Id: "LeftCenter", LegId: LeftCenter),
            new(Id: "LeftBack", LegId: LeftBack),
            new(Id: "RightFront", LegId: RightFront),
            new(Id: "RightCenter", LegId: RightCenter),
            new(Id: "RightBack", LegId: RightBack),
        };

    public static TheoryData<FootprintInput> DataFor_GivenStanceRadius_WhenGetFootprint_ThenLiesAtItsOwnLegsAzimuthOnTheStanceCircle()
    {
        // Expected footprints derived independently from the leg layout: each
        // leg stands at its own azimuth, on a circle of StanceRadius centred on
        // the thorax origin. Deliberately not derived from Thorax.TransformFor,
        // so an error in the mount geometry cannot make this test agree with a
        // matching error in the stance calculation.
        var diagonal = StanceRadius * (Math.Sqrt(2.0) / 2.0);

        return new()
        {
            new(
                Id: "LeftFront",
                LegId: LeftFront,
                ExpectedFootprint: new(diagonal, diagonal, Length.Zero)),
            new(
                Id: "LeftCenter",
                LegId: LeftCenter,
                ExpectedFootprint: new(Length.Zero, StanceRadius, Length.Zero)),
            new(
                Id: "LeftBack",
                LegId: LeftBack,
                ExpectedFootprint: new(-diagonal, diagonal, Length.Zero)),
            new(
                Id: "RightFront",
                LegId: RightFront,
                ExpectedFootprint: new(diagonal, -diagonal, Length.Zero)),
            new(
                Id: "RightCenter",
                LegId: RightCenter,
                ExpectedFootprint: new(Length.Zero, -StanceRadius, Length.Zero)),
            new(
                Id: "RightBack",
                LegId: RightBack,
                ExpectedFootprint: new(-diagonal, -diagonal, Length.Zero)),
        };
    }

    [Theory]
    [MemberData(nameof(DataForEachLeg))]
    public void GivenStanceRadius_WhenGetFootprint_ThenIsOnGroundPlane(LegInput input)
    {
        var footprint = Stance.GetFootprintInGroundFrameFor(input.LegId, StanceRadius);

        TestObjectFactory.AssertLengthEqual(
            input.Id, Length.Zero, footprint.Z, Precision);
    }

    [Theory]
    [MemberData(nameof(DataForEachLeg))]
    public void GivenStanceRadius_WhenGetFootprint_ThenIsThatRadiusFromThoraxOrigin(LegInput input)
    {
        var footprint = Stance.GetFootprintInGroundFrameFor(input.LegId, StanceRadius);

        TestObjectFactory.AssertLengthEqual(
            input.Id, StanceRadius, (footprint - Xyz.Zero).Length, Precision);
    }

    [Theory]
    [MemberData(nameof(DataFor_GivenStanceRadius_WhenGetFootprint_ThenLiesAtItsOwnLegsAzimuthOnTheStanceCircle))]
    public void GivenStanceRadius_WhenGetFootprint_ThenLiesAtItsOwnLegsAzimuthOnTheStanceCircle(FootprintInput input)
    {
        TestObjectFactory.AssertXyzEqual(
            input.Id,
            input.ExpectedFootprint,
            Stance.GetFootprintInGroundFrameFor(input.LegId, StanceRadius),
            Precision);
    }

    [Theory]
    [MemberData(nameof(DataForEachLeg))]
    public void GivenMountMovedFurtherFromOrigin_WhenGetFootprint_ThenStanceRadiusIsUnchanged(LegInput input)
    {
        // The stance radius is measured from the thorax origin, so it must not
        // drift when the mount geometry is re-measured. This is what made the
        // real robot stand at 22cm when 15cm was asked for.
        var mountDistanceFromOrigin = Thorax.TransformFor(input.LegId).Xyz.Length;
        var footprint = Stance.GetFootprintInGroundFrameFor(input.LegId, StanceRadius);

        Assert.True(
            mountDistanceFromOrigin > Length.Zero,
            $"{input.LegId} mount is expected to be offset from the origin");

        TestObjectFactory.AssertLengthEqual(
            input.Id, StanceRadius, footprint.Length, Precision);
    }

    [Theory]
    [MemberData(nameof(DataForEachLeg))]
    public void GivenStandingStill_WhenGetTipPositions_ThenAllTipsAreGroundClearanceBelowThorax(LegInput input)
    {
        var tipPositions = Stance.GetTipPositionsInThoraxFrameFor(StandingStill);

        TestObjectFactory.AssertLengthEqual(
            input.Id, -GroundClearance, tipPositions[input.LegId].Z, Precision);
    }

    [Fact]
    public void GivenStandingStill_WhenGetTipPositions_ThenTipsAreSymmetricAroundThorax()
    {
        var tipPositions = Stance.GetTipPositionsInThoraxFrameFor(StandingStill);

        var mean = tipPositions.Mean();

        TestObjectFactory.AssertXyzEqual(
            "symmetry",
            Xyz.Zero with { Z = -GroundClearance },
            mean, Precision);
    }

    [Theory]
    [MemberData(nameof(DataForEachLeg))]
    public void GivenLeanForward1cm_WhenGetTipPositions_ThenAllTipsMoveBackward1cm(LegInput input)
    {
        var offsetX = Length.FromCentimeters(1);

        var standingTips = Stance.GetTipPositionsInThoraxFrameFor(StandingStill);

        var leanedForward = MotionWith(
            Transform.FromXyz(new Xyz(offsetX, Length.Zero, GroundClearance)));
        var leanedTips = Stance.GetTipPositionsInThoraxFrameFor(leanedForward);

        TestObjectFactory.AssertXyzEqual(
            input.Id,
            standingTips[input.LegId] with { X = standingTips[input.LegId].X - offsetX },
            leanedTips[input.LegId],
            Precision);
    }

    [Theory]
    [MemberData(nameof(DataForEachLeg))]
    public void GivenLeanUp1cm_WhenGetTipPositions_ThenAllTipsMoveDown1cm(LegInput input)
    {
        var offsetZ = Length.FromCentimeters(1);

        var standingTips = Stance.GetTipPositionsInThoraxFrameFor(StandingStill);

        var leanedUp = MotionWith(
            Transform.FromXyz(Xyz.Zero with { Z = GroundClearance + offsetZ }));
        var leanedTips = Stance.GetTipPositionsInThoraxFrameFor(leanedUp);

        TestObjectFactory.AssertXyzEqual(
            input.Id,
            standingTips[input.LegId] with { Z = standingTips[input.LegId].Z - offsetZ },
            leanedTips[input.LegId],
            Precision);
    }

    [Theory]
    [MemberData(nameof(DataForEachLeg))]
    public void GivenLargerStanceRadius_WhenGetTipPositions_ThenTipsAreThatRadiusFromThoraxOrigin(LegInput input)
    {
        var widerRadius = StanceRadius + Length.FromCentimeters(3);

        var widerMotion = StandingStill with { StanceRadius = widerRadius };
        var widerTips = Stance.GetTipPositionsInThoraxFrameFor(widerMotion);

        var tipXy = widerTips[input.LegId] with { Z = Length.Zero };

        TestObjectFactory.AssertLengthEqual(
            input.Id, widerRadius, tipXy.Length, Precision);
    }

    [Theory]
    [MemberData(nameof(DataForEachLeg))]
    public void GivenStanceRadius_WhenGetFootprints_ThenAllLegsAreEquallyFarFromThoraxOrigin(LegInput input)
    {
        // Center legs are mounted closer to the origin than front and back legs,
        // so adding the radius to the mount offset would make the stance an
        // irregular hexagon rather than a circle.
        var footprint = Stance.GetFootprintInGroundFrameFor(input.LegId, StanceRadius);

        TestObjectFactory.AssertLengthEqual(
            input.Id, StanceRadius, footprint.Length, Precision);
    }

    [Fact]
    public void GivenPositiveRollLean_WhenGetTipPositions_ThenLeftTipIsFurtherBelowThoraxThanRightTip()
    {
        var rollLean = MotionWith(new Transform(
            Xyz.Zero with { Z = GroundClearance },
            Q.FromRpy(new Rpy(0.05, 0, 0))));

        var tipPositions = Stance.GetTipPositionsInThoraxFrameFor(rollLean);

        // A positive roll around x raises the left, meaning positive y, side of
        // the thorax, so the left tip ends up further below it.
        Assert.True(
            tipPositions[LeftCenter].Z < tipPositions[RightCenter].Z,
            TestObjectFactory.TestMessage(
                "roll", tipPositions[LeftCenter], tipPositions[RightCenter]));
    }

    [Fact]
    public void GivenSameMotion_WhenGetTipPositionsTwice_ThenResultIsIdentical()
    {
        var first = Stance.GetTipPositionsInThoraxFrameFor(StandingStill);
        var second = Stance.GetTipPositionsInThoraxFrameFor(StandingStill);

        Assert.Equal(first, second);
    }

    public record LegInput(string Id, MaydayLegId LegId);

    public record FootprintInput(string Id, MaydayLegId LegId, Xyz ExpectedFootprint);
}
