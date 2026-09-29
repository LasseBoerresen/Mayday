using MaydayDomain.Components;
using RobotDomain.Geometry;
using UnitsNet;

namespace MaydayDomain.MotionPlanning;

/// <summary>
/// Pure geometry for the static part of a motion, meaning the stance radius and
/// the lean, but not the twist, which represents movement away from one spot.
/// </summary>
/// <remarks>
/// Ground is assumed flat and is defined as z = 0 in the ground frame. That
/// makes the tip positions a direct function of the goal motion, so no ground
/// plane estimation or per leg clearance measurement is needed.
/// </remarks>
public static class Stance
{
    /// <summary>
    /// Where the given leg is supposed to stand on the ground, at the given
    /// stance radius from the thorax origin, in the leg's own outward direction.
    /// </summary>
    /// <remarks>
    /// The stance radius is measured from the thorax origin, not from the coxa
    /// motor mount, so all six footprints lie on one circle centred on the body
    /// and the radius means the same thing for center legs as for front and
    /// back legs. Each mount is yawed to face directly away from the origin, so
    /// rotating the x axis by the mount rotation gives the outward radial
    /// direction.
    /// </remarks>
    public static Xyz GetFootprintInGroundFrameFor(MaydayLegId legId, Length stanceRadius)
    {
        var outwards = Thorax.TransformFor(legId).Q.Rotate(Xyz.Zero with { X = stanceRadius });

        return outwards with { Z = Length.Zero };
    }

    /// <summary>
    /// The thorax frame position of the tip of the given leg, for the footprint
    /// and thorax pose described by the motion.
    /// </summary>
    public static Xyz GetTipPositionInThoraxFrameFor(MaydayLegId legId, Motion motion)
    {
        var footprint = GetFootprintInGroundFrameFor(legId, motion.StanceRadius);

        // Subtracting the lean expresses the footprint relative to the leaned
        // thorax, which includes rotating it into the thorax frame.
        return (Transform.FromXyz(footprint) - motion.Lean).Xyz;
    }

    /// <summary>
    /// The thorax frame tip positions of all legs, for the given motion.
    /// </summary>
    public static MaydayStructureSet<Xyz> GetTipPositionsInThoraxFrameFor(Motion motion)
    {
        return MaydayLegId.AllLegIds
            .ToDictionary(
                legId => legId,
                legId => GetTipPositionInThoraxFrameFor(legId, motion))
            .ToMaydayStructureSet();
    }
}
