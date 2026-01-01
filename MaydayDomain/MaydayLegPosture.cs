using System.Collections.Immutable;
using System.Numerics;
using UnitsNet;
using UnitsNet.Units;
using static System.Math;

namespace MaydayDomain;

public record MaydayLegPosture(Angle CoxaAngle, Angle FemurAngle, Angle TibiaAngle)
{
    public MaydayLegPosture(double coxaAngle, double femurAngle, double tibiaAngle)
        : this(Angle.FromRevolutions(coxaAngle), Angle.FromRevolutions(femurAngle), Angle.FromRevolutions(tibiaAngle)) {}

    public MaydayLegPosture(IEnumerable<Angle> angles)
        : this(angles.Skip(0).First(), angles.Skip(1).First(), angles.Skip(2).First()) {}
    
    public static MaydayLegPosture Neutral => new(0.0, 0.0, 0.0);
    public static MaydayLegPosture NeutralWithStraightFemur => new(0.0, 0.0625, 0.0);
    public static MaydayLegPosture NeutralWithBackTwist => new(0.125, 0.0, 00.0);
    public static MaydayLegPosture Straight => new(0.0, 0.0625, 0.25-0.0625);
    public static MaydayLegPosture StraightWithBackTwist => new(0.125, 0.0625, 0.25-0.0625);
    public static MaydayLegPosture Sitting => new(0.0, -0.3, -0.2);
    public static MaydayLegPosture SittingTall => new(0.0, -0.3, -0.15);
    public static MaydayLegPosture Standing => new(0.0, -0.21, -0.26);
    public static MaydayLegPosture StandingHigh => new(0.0, 0.35, 0.3);
    public static MaydayLegPosture StandingWide => new(0.0, 0.1, -0.1);

    public IImmutableList<Angle> AsListOfGoalAngles() => [CoxaAngle, FemurAngle, TibiaAngle];

    public static MaydayLegPosture FromSines(
        double coxaSin, double coxaCos, 
        double femurSin, double femurCos, 
        double tibiaSin, double tibiaCos)
    {
        return new(
            CoxaAngle: Angle.FromRadians(Atan2(coxaSin, coxaCos)),
            FemurAngle: Angle.FromRadians(Atan2(femurSin, femurCos)),
            TibiaAngle: Angle.FromRadians(Atan2(tibiaSin, tibiaCos)));
    }
    
    public override string ToString() => $"[Coxa: {CoxaAngle,6:F3}, Femur: {FemurAngle,6:F3}, Tibia: {TibiaAngle,6:F3}]";

    public static MaydayLegPosture operator -(MaydayLegPosture a, MaydayLegPosture b)
    {
        return new(a.CoxaAngle - b.CoxaAngle, a.FemurAngle - b.FemurAngle, a.TibiaAngle - b.TibiaAngle);
    }

    public Angle DistanceTo(MaydayLegPosture other)
    {
        // Not sure if this is the right way to compare two 3d angles. 
        var squaredRadiansSum = 
              Pow(Sin(CoxaAngle.Radians) - Sin(other.CoxaAngle.Radians), 2)
            + Pow(Sin(FemurAngle.Radians) - Sin(other.FemurAngle.Radians), 2)
            + Pow(Sin(TibiaAngle.Radians) - Sin(other.TibiaAngle.Radians), 2)
            + Pow(Cos(CoxaAngle.Radians) - Cos(other.CoxaAngle.Radians), 2)
            + Pow(Cos(FemurAngle.Radians) - Cos(other.FemurAngle.Radians), 2)
            + Pow(Cos(TibiaAngle.Radians) - Cos(other.TibiaAngle.Radians), 2);

        // Not sure if this is actually radians anymore 
        var radiansDistance = Pow(squaredRadiansSum, 1 / 2.0);
        
        return Angle.FromRadians(radiansDistance);
    }

    /// <summary>
    /// Interpolate between two rpy taking the circular nature of the angles into account
    /// </summary>
    public static MaydayLegPosture InterpolateBetween(MaydayLegPosture start, MaydayLegPosture end, Ratio fraction)
    {
        ValidateFractionRange(fraction);
        
        var t = fraction.DecimalFractions;

        var interpolated = FromSines(
            coxaSin: LinearInterpolate(Sin(start.CoxaAngle.Radians), Sin(end.CoxaAngle.Radians), t),
            coxaCos: LinearInterpolate(Cos(start.CoxaAngle.Radians), Cos(end.CoxaAngle.Radians), t),
            femurSin: LinearInterpolate(Sin(start.FemurAngle.Radians), Sin(end.FemurAngle.Radians), t),
            femurCos: LinearInterpolate(Cos(start.FemurAngle.Radians), Cos(end.FemurAngle.Radians), t),
            tibiaSin: LinearInterpolate(Sin(start.TibiaAngle.Radians), Sin(end.TibiaAngle.Radians), t),
            tibiaCos: LinearInterpolate(Cos(start.TibiaAngle.Radians), Cos(end.TibiaAngle.Radians), t)
        );
        
        // So the display unit is revolutions, not radians
        return interpolated.ToRevolutions();
    }

    /// <summary>
    /// After intermediate calculations, the unit can end up as something else than revolutions, which can be confusing. 
    /// </summary>
    MaydayLegPosture ToRevolutions()
    {
        return new(
            CoxaAngle.ToUnit(AngleUnit.Revolution),
            FemurAngle.ToUnit(AngleUnit.Revolution),
            TibiaAngle.ToUnit(AngleUnit.Revolution));
    }

    private static double LinearInterpolate(double start, double end, double t) => start + (end - start) * t;

    private static void ValidateFractionRange(Ratio fraction)
    {
        if (fraction.DecimalFractions < 0.0 || fraction.DecimalFractions > 1.0)
            throw new ArgumentException($"Fraction must be between 0.0 and 1.0, got: {fraction}");
    }
}
