using RobotDomain.Geometry;

namespace MaydayDomain.Internal;

/// <summary>
/// A <see cref="LegPostureByPositionMap"/> for a leg that is the mirror image, in Y of its own frame, of the leg the
/// wrapped map was built for: it returns the wrapped map's posture for the tip position mirrored in Y.
/// </summary>
public class MirroredInYLegPostureByPositionMap(LegPostureByPositionMap map) : LegPostureByPositionMap
{
    /// <inheritdoc />
    public MaydayLegPosture GetFor(Xyz tipPosition, MaydayLegPosture currentPosture)
    {
        // The current posture needs no mirroring: the same joint values place the mirrored leg's tip at the mirror of
        // where they place the original leg's tip, so closeness between postures is unchanged.
        return map.GetFor(tipPosition with { Y = -tipPosition.Y }, currentPosture);
    }
}
