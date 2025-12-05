using UnitsNet;

namespace RobotDomain.Geometry;

public record XyzSpeed(Speed X, Speed Y, Speed Z)
{
    public static XyzSpeed Zero => new(Speed.Zero, Speed.Zero, Speed.Zero);
}
