using RobotDomain.Geometry;
using UnitsNet;

namespace MaydayDomain.MotionPlanning;

/// <param name="Twist">Movement away from the current spot.</param>
/// <param name="Lean">Pose of the thorax relative to the ground.</param>
/// <param name="StanceRadius">
/// Radius of the circle of footprints, measured from the thorax origin, not
/// from the individual coxa motor mounts.
/// </param>
public record Motion(Twist Twist, Transform Lean, Length StanceRadius)
{
    public static Motion StandingStill => 
        new(    
            Twist: Twist.Zero,
            Lean: Transform.Zero with { Xyz = Xyz.Zero with { Z = Length.FromMeters(0.1) } },
            StanceRadius: Length.FromMeters(0.15)); // default mayday standing stance
}