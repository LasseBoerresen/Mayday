using RobotDomain.Geometry;
using UnitsNet;

namespace MaydayDomain.MotionPlanning;

public record Motion(Twist Twist, Transform Lean, Length StanceRadius)
{
    public static Motion StandingStill => 
        new(    
            Twist: Twist.Zero,
            Lean: Transform.Zero with { Xyz = Xyz.Zero with { Z = Length.FromMeters(0.1) } },
            StanceRadius: Length.FromMeters(0.15)); // default mayday standing stance
}