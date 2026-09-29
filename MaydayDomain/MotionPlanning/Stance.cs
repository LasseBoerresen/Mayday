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
    /// Where the given leg is supposed to stand on the ground, radially
    /// outwards from its own coxa motor mount, at the given stance radius.
    /// </summary>
    public static Xyz GetFootprintInGroundFrameFor(MaydayLegId legId, Length stanceRadius)
    {
        var mount = Thorax.TransformFor(legId);
        var outwards = mount.Q.Rotate(Xyz.Zero with { X = stanceRadius });

        return (mount.Xyz + outwards) with { Z = Length.Zero };
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
