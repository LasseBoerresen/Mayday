using UnitsNet;

namespace Domain.Geometry;

public record Xyz(Length X, Length Y, Length Z)
{
    public Xyz(double x, double y, double z)
        : this(Length.FromMeters(x), Length.FromMeters(y), Length.FromMeters(z)) {}

    public static Xyz Zero => new(0, 0, 0);
    public static Xyz One => new(1, 1, 1);

    public static Xyz operator *(Xyz xyz, double multiplier)
    {
        return new(xyz.X * multiplier, xyz.Y * multiplier, xyz.Z * multiplier);
    }
}
