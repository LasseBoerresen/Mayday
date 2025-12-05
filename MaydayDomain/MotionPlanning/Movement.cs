using Microsoft.VisualBasic;
using RobotDomain.Geometry;
using UnitsNet;

namespace MaydayDomain.MotionPlanning;

public record Movement(
    Twist Twist, 
    Transform Lean,
    Length StanceRadius,
    Duration Duration,
    DateTimeOffset TimeStamp)
{
    public static Movement Zero(DateTimeOffset now)
    {
        return new(
            Twist: Twist.Zero, 
            Lean: Transform.Zero, 
            StanceRadius: Length.FromMeters(0.15), // default mayday standing stance 
            Duration: Duration.Zero, 
            TimeStamp: now);
    }
}