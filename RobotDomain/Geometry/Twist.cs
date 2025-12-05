namespace RobotDomain.Geometry;

/// <summary>
/// Describes a velocity in 3d, both translation and rotation
/// </summary>
public record Twist(XyzSpeed Xyz, RpySpeed Rpy)
{
    public static Twist Zero => new(XyzSpeed.Zero, RpySpeed.Zero);
}
