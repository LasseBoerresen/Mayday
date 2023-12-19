using UnitsNet;

namespace mayday.Geometry;

public record Xyz(Length X, Length Y, Length Z) : Vec3<Length>(X, Y, Z)
{
    public Xyz(double x, double y, double z)
        : this(Length.FromMeters(x), Length.FromMeters(y), Length.FromMeters(z)) {}

    public static Xyz Zero => new(0, 0, 0);
}
