using RobotDomain.Geometry;

namespace MaydayDomain.Components;

public record Coxa
{
    public static Xyz FemurMotorMountTranslation => new(0.033, 0.0, -0.013);
}