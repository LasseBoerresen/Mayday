using UnitsNet;
using UnitsNet.Units;

namespace mayday.Geometry;

public record Rpy(Angle R, Angle P, Angle Y)
{
    public Rpy(double r, double p, double y)
        : this(Angle.FromRevolutions(r), Angle.FromRevolutions(p), Angle.FromRevolutions(y)) {}

    public static Rpy Zero => new(0, 0, 0);

    public static Rpy One => new(1, 1, 1);

    public static Rpy operator *(Rpy rpy, double multiplier)
    {
        return new(rpy.R * multiplier, rpy.P * multiplier, rpy.Y * multiplier);
    }

    // public override string ToString()
    // {
        // return $"{nameof(Rpy)} {{ {nameof(R)} = {R.Revolutions} τ, {P.Revolutions} τ, {Y.Revolutions} τ }}";
    // }
}
