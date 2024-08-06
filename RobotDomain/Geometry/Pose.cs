using UnitsNet;

namespace RobotDomain.Geometry;

public record Pose(Xyz Xyz, Q Q)
{
    public static Pose Zero => new(Xyz.Zero, Q.Unit);
    public static Pose FromXyz(Xyz xyz) => new(xyz, Q.Unit);
    public static Pose FromQ(Q q) => new(Xyz.Zero, q);

    public static Pose operator *(Pose pose, double multiplier)
    {
        return new(pose.Xyz * multiplier, pose.Q * multiplier);
    }
    
    public static Pose operator +(Pose a, Pose b)
    {
        return new(a.Xyz + b.Xyz, a.Q + b.Q);
    }

    public static Pose Add(Pose a, Pose b)
    {
        return new(a.Xyz + b.Xyz, a.Q + b.Q);
    }

    public bool IsAlmostEqual(Pose other, Length translationPrecision, Angle rotationalPrecision)
    {
        var translation = Xyz.IsAlmostEqual(other.Xyz, translationPrecision);
        var rotation = Q.IsOrientationAlmostEqual(other.Q, rotationalPrecision.Revolutions);
        
        return translation && rotation;
    }
}
