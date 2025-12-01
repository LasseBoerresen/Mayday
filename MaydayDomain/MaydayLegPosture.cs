using System.Collections.Immutable;
using System.Numerics;
using UnitsNet;
using static System.Math;

namespace MaydayDomain;

public record MaydayLegPosture(Angle CoxaAngle, Angle FemurAngle, Angle TibiaAngle)
{
    MaydayLegPosture(double coxaAngle, double femurAngle, double tibiaAngle)
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
        float coxaSin, float coxaCos, 
        float femurSin, float femurCos, 
        float tibiaSin, float tibiaCos)
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
              Pow(Sin(CoxaAngle.Radians - other.CoxaAngle.Radians), 2)
            + Pow(Sin(FemurAngle.Radians - other.FemurAngle.Radians), 2)
            + Pow(Sin(TibiaAngle.Radians - other.TibiaAngle.Radians), 2)
            + Pow(Cos(CoxaAngle.Radians - other.CoxaAngle.Radians), 2)
            + Pow(Cos(FemurAngle.Radians - other.FemurAngle.Radians), 2)
            + Pow(Cos(TibiaAngle.Radians - other.TibiaAngle.Radians), 2);

        // Not sure if this is actually radians anymore 
        var radiansDistance = Pow(squaredRadiansSum, 1 / 2.0);
        
        return Angle.FromRadians(radiansDistance);
    }
}
