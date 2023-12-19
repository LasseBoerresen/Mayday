namespace mayday.Geometry;

public record Pose(Xyz Xyz, Rpy Rpy)
{
    public static Pose Zero => new(Xyz.Zero, Rpy.Zero);
}
