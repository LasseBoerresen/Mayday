namespace mayday.Maths;
using UnitsNet;

public record Vec3<T>(T X0, T X1, T X2) where T : IQuantity;

public record Xyz(Length X, Length Y, Length Z) : Vec3<Length>(X, Y, Z)
{
    public Xyz(double x, double y, double z)
        : this(Length.FromMeters(x), Length.FromMeters(y), Length.FromMeters(z)) {}
}

public record Rpy(Angle R, Angle P, Angle Y) : Vec3<Angle>(R, P, Y)
{
    public Rpy(double x, double y, double z)
        : this(Angle.FromRadians(x), Angle.FromRadians(y), Angle.FromRadians(z)) {}
}
