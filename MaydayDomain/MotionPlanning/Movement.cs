using Microsoft.VisualBasic;
using RobotDomain.Geometry;
using UnitsNet;

namespace MaydayDomain.MotionPlanning;

public record Movement(Twist Twist, Transform Lean, Length StanceRadius)
{
    public static Movement StandingStill => 
        new(    
            Twist: Twist.Zero,
            Lean: Transform.Zero with { Xyz = Xyz.Zero with { Z = Length.FromMeters(0.1) } },
            StanceRadius: Length.FromMeters(0.15)); // default mayday standing stance
}