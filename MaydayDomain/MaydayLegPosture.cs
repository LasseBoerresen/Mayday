﻿using System.Collections.Immutable;
using UnitsNet;

namespace MaydayDomain;

public record MaydayLegPosture(Angle CoxaAngle, Angle FemurAngle, Angle TibiaAngle)
{
    MaydayLegPosture(double coxaAngle, double femurAngle, double tibiaAngle)
        : this(Angle.FromRevolutions(coxaAngle), Angle.FromRevolutions(femurAngle), Angle.FromRevolutions(tibiaAngle)) {}

    public MaydayLegPosture(IEnumerable<Angle> angles)
        : this(angles.Skip(0).First(), angles.Skip(1).First(), angles.Skip(2).First()) {}
    
    public static MaydayLegPosture Neutral => new(0.0, 0.0, 0.0);
    public static MaydayLegPosture NeutralWithBackTwist => new(0.125, 0.0, 00.0);
    public static MaydayLegPosture Straight => new(0.0, 0.0625, 0.25); // TODO Should be non negative, moving femur down is a positive rotation
    public static MaydayLegPosture StraightWithBackTwist => new(0.125, 0.0625, 0.25);
    public static MaydayLegPosture Sitting => new(0.0, -0.3, -0.2);
    public static MaydayLegPosture SittingTall => new(0.0, -0.3, -0.15);
    public static MaydayLegPosture Standing => new(0.0, -0.21, -0.26);
    public static MaydayLegPosture StandingHigh => new(0.0, 0.35, 0.3);
    public static MaydayLegPosture StandingWide => new(0.0, 0.1, -0.1);

    public IImmutableList<Angle> AsListOfGoalAngles() => [CoxaAngle, FemurAngle, TibiaAngle];
}
