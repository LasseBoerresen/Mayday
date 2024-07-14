namespace RobotDomain.Geometry;

public record Pose(Xyz Xyz, Rpy Rpy)
{
    public static Pose Zero => new(Xyz.Zero, Rpy.Zero);
    public static Pose One => new(Xyz.One, Rpy.One);
    public static Pose FromXyz(Xyz xyz) => new(xyz, Rpy.Zero);
    public static Pose FromRpy(Rpy rpy) => new(Xyz.Zero, rpy);

    public static Pose operator *(Pose pose, double multiplier)
    {
        return new(pose.Xyz * multiplier, pose.Rpy * multiplier);
    }

    public static Pose Add(Pose a, Pose b)
    {
        return new(a.Xyz + b.Xyz, a.Rpy + b.Rpy);
    }
}
