using UnitsNet;

namespace Domain;

public record MaydayLegPosture(Angle CoxaAngle, Angle FemurAngle, Angle TibiaAngle)
{
    private MaydayLegPosture(double coxa, double femur, double tibia)
        : this(Angle.FromRevolutions(coxa), Angle.FromRevolutions(femur), Angle.FromRevolutions(tibia)) {}

    public MaydayLegPosture(IEnumerable<Angle> angles)
        : this(angles.Skip(0).First(), angles.Skip(1).First(), angles.Skip(2).First()) {}
    
    public static MaydayLegPosture Neutral => new(0.0, 0.0, 0.0);
    public static MaydayLegPosture Sitting => new(0.0, 0.3, -0.2);
    public static MaydayLegPosture Standing => new(0.0, 0.2, -0.25);
    public static MaydayLegPosture StandingWide => new(0.0, 0.1, -0.1);
    
    public IEnumerable<Angle> AsEnumerable() => [CoxaAngle, FemurAngle, TibiaAngle];
}
