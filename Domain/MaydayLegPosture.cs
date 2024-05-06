using UnitsNet;

namespace Domain;

public record MaydayLegPosture
{
    public MaydayLegPosture(Angle coxa, Angle femur, Angle tibia) {}

    public MaydayLegPosture(double coxa, double femur, double tibia)
        : this(Angle.FromRevolutions(coxa), Angle.FromRevolutions(femur), Angle.FromRevolutions(tibia)) {}

    public static MaydayLegPosture Neutral => new(0.0, 0.0, 0.0);
    public static MaydayLegPosture Sitting => new(0.0, 0.3, -0.2);
    public static MaydayLegPosture Standing => new(0.0, 0.2, -0.25);
    public static MaydayLegPosture StandingWide => new(0.0, 0.1, -0.1);
}
