using UnitsNet;

namespace RobotDomain.Geometry;

public record RpySpeed(RotationalSpeed R, RotationalSpeed P, RotationalSpeed Y)
{
    public static RpySpeed Zero 
        => new RpySpeed(RotationalSpeed.Zero, RotationalSpeed.Zero, RotationalSpeed.Zero);
}
