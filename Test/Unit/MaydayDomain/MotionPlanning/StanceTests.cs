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

    [Fact]
    public void GivenStanceRadius_WhenGetFootprint_ThenIsOnGroundPlane()
    {
        foreach (var legId in AllLegIds)
        {
            var footprint = Stance.GetFootprintInGroundFrameFor(legId, StanceRadius);

            TestObjectFactory.AssertLengthEqual(
                legId.ToString(), Length.Zero, footprint.Z, Precision);
        }
    }

    [Fact]
    public void GivenStanceRadius_WhenGetFootprint_ThenIsThatRadiusFromThoraxOrigin()
    {
        foreach (var legId in AllLegIds)
        {
            var footprint = Stance.GetFootprintInGroundFrameFor(legId, StanceRadius);

            TestObjectFactory.AssertLengthEqual(
                legId.ToString(), StanceRadius, (footprint - Xyz.Zero).Length, Precision);
        }
    }

    [Fact]
    public void GivenStanceRadius_WhenGetFootprint_ThenLiesAtItsOwnLegsAzimuthOnTheStanceCircle()
    {
        // Expected footprints derived independently from the leg layout: each
        // leg stands at its own azimuth, on a circle of StanceRadius centred on
        // the thorax origin. Deliberately not derived from Thorax.TransformFor,
        // so an error in the mount geometry cannot make this test agree with a
        // matching error in the stance calculation.
        var diagonal = StanceRadius * (Math.Sqrt(2.0) / 2.0);

        Dictionary<MaydayLegId, Xyz> expectedFootprints = new()
        {
            { LeftFront, new(diagonal, diagonal, Length.Zero) },
            { LeftCenter, new(Length.Zero, StanceRadius, Length.Zero) },
            { LeftBack, new(-diagonal, diagonal, Length.Zero) },
            { RightFront, new(diagonal, -diagonal, Length.Zero) },
            { RightCenter, new(Length.Zero, -StanceRadius, Length.Zero) },
            { RightBack, new(-diagonal, -diagonal, Length.Zero) },
        };

        foreach (var (legId, expected) in expectedFootprints)
            TestObjectFactory.AssertXyzEqual(
                legId.ToString(),
                expected,
                Stance.GetFootprintInGroundFrameFor(legId, StanceRadius),
                Precision);
    }

    [Fact]
    public void GivenMountMovedFurtherFromOrigin_WhenGetFootprint_ThenStanceRadiusIsUnchanged()
    {
        // The stance radius is measured from the thorax origin, so it must not
        // drift when the mount geometry is re-measured. This is what made the
        // real robot stand at 22cm when 15cm was asked for.
        foreach (var legId in AllLegIds)
        {
            var mountDistanceFromOrigin = Thorax.TransformFor(legId).Xyz.Length;
            var footprint = Stance.GetFootprintInGroundFrameFor(legId, StanceRadius);

            Assert.True(
                mountDistanceFromOrigin > Length.Zero,
                $"{legId} mount is expected to be offset from the origin");

            TestObjectFactory.AssertLengthEqual(
                legId.ToString(), StanceRadius, footprint.Length, Precision);
        }
    }

    [Fact]
    public void GivenStandingStill_WhenGetTipPositions_ThenAllTipsAreGroundClearanceBelowThorax()
    {
        var tipPositions = Stance.GetTipPositionsInThoraxFrameFor(StandingStill);

        foreach (var legProperty in tipPositions.ToLegProperties())
            TestObjectFactory.AssertLengthEqual(
                legProperty.LegId.ToString(), -GroundClearance, legProperty.Value.Z, Precision);
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

    [Fact]
    public void GivenLeanForward1cm_WhenGetTipPositions_ThenAllTipsMoveBackward1cm()
    {
        var offsetX = Length.FromCentimeters(1);

        var standingTips = Stance.GetTipPositionsInThoraxFrameFor(StandingStill);

        var leanedForward = MotionWith(
            Transform.FromXyz(new Xyz(offsetX, Length.Zero, GroundClearance)));
        var leanedTips = Stance.GetTipPositionsInThoraxFrameFor(leanedForward);

        var expectedTips = standingTips.Map(tip => tip with { X = tip.X - offsetX });

        AssertTipsEqual(expectedTips, leanedTips);
    }

    [Fact]
    public void GivenLeanUp1cm_WhenGetTipPositions_ThenAllTipsMoveDown1cm()
    {
        var offsetZ = Length.FromCentimeters(1);

        var standingTips = Stance.GetTipPositionsInThoraxFrameFor(StandingStill);

        var leanedUp = MotionWith(
            Transform.FromXyz(Xyz.Zero with { Z = GroundClearance + offsetZ }));
        var leanedTips = Stance.GetTipPositionsInThoraxFrameFor(leanedUp);

        var expectedTips = standingTips.Map(tip => tip with { Z = tip.Z - offsetZ });

        AssertTipsEqual(expectedTips, leanedTips);
    }

    [Fact]
    public void GivenLargerStanceRadius_WhenGetTipPositions_ThenTipsAreThatRadiusFromThoraxOrigin()
    {
        var widerRadius = StanceRadius + Length.FromCentimeters(3);

        var widerMotion = StandingStill with { StanceRadius = widerRadius };
        var widerTips = Stance.GetTipPositionsInThoraxFrameFor(widerMotion);

        foreach (var legProperty in widerTips.ToLegProperties())
        {
            var tipXy = legProperty.Value with { Z = Length.Zero };

            TestObjectFactory.AssertLengthEqual(
                legProperty.LegId.ToString(), widerRadius, tipXy.Length, Precision);
        }
    }

    [Fact]
    public void GivenStanceRadius_WhenGetFootprints_ThenAllLegsAreEquallyFarFromThoraxOrigin()
    {
        // Center legs are mounted closer to the origin than front and back legs,
        // so adding the radius to the mount offset would make the stance an
        // irregular hexagon rather than a circle.
        var radii = AllLegIds
            .Select(legId => Stance.GetFootprintInGroundFrameFor(legId, StanceRadius).Length)
            .ToList();

        foreach (var radius in radii)
            TestObjectFactory.AssertLengthEqual("radius", radii.First(), radius, Precision);
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

    static void AssertTipsEqual(MaydayStructureSet<Xyz> expected, MaydayStructureSet<Xyz> actual)
    {
        foreach (var legId in AllLegIds)
            TestObjectFactory.AssertXyzEqual(legId.ToString(), expected[legId], actual[legId], Precision);
    }
}
