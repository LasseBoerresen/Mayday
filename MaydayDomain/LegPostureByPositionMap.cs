using RobotDomain.Geometry;

namespace MaydayDomain;

public interface LegPostureByPositionMap
{
    /// <summary>
    /// Get leg joint posture which results in the given tip position 
    /// </summary>
    /// <returns>Posture for tip position closest to current posture</returns>
    /// <exception cref="InvalidOperationException">If the tip position is unreachable</exception>
    MaydayLegPosture GetFor(Xyz tipPosition, MaydayLegPosture currentPosture);
}
