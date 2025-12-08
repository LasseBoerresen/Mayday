using Generic;
using LanguageExt;
using static System.Math;
using static Generic.UnitsNetExtensions;
using Length = UnitsNet.Length;

namespace RobotDomain.Geometry;

public record Xyz(Length X, Length Y, Length Z)
{
    // TODO Refactor this meters constructor to factoryMethod Called Meters
    public Xyz(double x, double y, double z)
        : this(Length.FromMeters(x), Length.FromMeters(y), Length.FromMeters(z)) {}

    public static Xyz Zero => new(0, 0, 0);
    public static Xyz One => new(1, 1, 1);

    public Length Length
    {
        get
        {
            return Length.FromMeters(
                Pow(
                    Pow(X.Meters, 2)
                    + Pow(X.Meters, 2)
                    + Pow(X.Meters, 2),
                    1 / 2.0));
        }
    }

    /// <summary>
    /// Every coordinate has random value between [-1m:1m]
    /// </summary>
    public static Xyz Random()
    {
        return Random(max: Length.FromMeters(1));
    }
    
    public static Xyz Random(Length max)
    {
        Random random = new();
        
        return new(
            X: max * random.NextDoubleNeg1ToPos1(), 
            Y: max * random.NextDoubleNeg1ToPos1(), 
            Z: max * random.NextDoubleNeg1ToPos1());
    }

    public static Xyz operator +(Xyz a, Xyz b)
    {
        return new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    
    public static Xyz operator -(Xyz a, Xyz b)
    {
        return new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    
    public static Xyz operator -(Xyz a)
    {
        return new(-a.X, -a.Y, -a.Z);
    }

    public static Xyz operator *(Xyz xyz, double multiplier)
    {
        return new(xyz.X * multiplier, xyz.Y * multiplier, xyz.Z * multiplier);
    }

    public bool IsAlmostEqual(Xyz other, Length precision)
    {
        return UnitsNetExtensions.IsAlmostEqual(X, other.X, precision)
            && UnitsNetExtensions.IsAlmostEqual(Y, other.Y, precision)
            && UnitsNetExtensions.IsAlmostEqual(Z, other.Z, precision);
    }
    
    public override string ToString()
    {
        return $"[X: {X.Meters,6:F3}, Y: {Y.Meters,6:F3}, Z: {Z.Meters,6:F3}]";
    }
}
