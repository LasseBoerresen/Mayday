using UnitsNet;

namespace Domain;

public record MaydayLegPosture(Angle CoxaAngle, Angle FemurAngle, Angle TibiaAngle)
{
    private MaydayLegPosture(double coxa, double femur, double tibia)
        : this(Angle.FromRevolutions(coxa), Angle.FromRevolutions(femur), Angle.FromRevolutions(tibia)) {}

    public MaydayLegPosture(Angle[] angles)
        : this(angles[0], angles[1], angles[2]) {}
    
    public static MaydayLegPosture Neutral => new(0.0, 0.0, 0.0);
    public static MaydayLegPosture Sitting => new(0.0, 0.3, -0.2);
    public static MaydayLegPosture Standing => new(0.0, 0.2, -0.25);
    public static MaydayLegPosture StandingWide => new(0.0, 0.1, -0.1);
    
    public IEnumerable<Angle> AsEnumerable() => [CoxaAngle, FemurAngle, TibiaAngle];
}
