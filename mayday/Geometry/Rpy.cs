using UnitsNet;

namespace mayday.Geometry;

public record Rpy(Angle R, Angle P, Angle Y) : Vec3<Angle>(R, P, Y)
{
    public Rpy(double x, double y, double z)
        : this(Angle.FromRadians(x), Angle.FromRadians(y), Angle.FromRadians(z)) {}

    public static Rpy Zero => new(0, 0, 0);
}
