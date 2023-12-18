using System.Numerics;
using mayday.Maths;
using UnitsNet;

namespace mayday.mayday;

public record MaydayLegPosture(Angle Coxa, Angle Femur, Angle Tibia) : Rpy(Coxa, Femur, Tibia)
{
    public MaydayLegPosture(double coxa, double femur, double tibia)
        : this(Angle.FromRadians(coxa), Angle.FromRadians(femur), Angle.FromRadians(tibia)) {}

    public static MaydayLegPosture Neutral => new(0, 0, 0);
    public static MaydayLegPosture Sitting => new(0, Math.Tau * 0.3, -Math.Tau * 0.2);
    public static MaydayLegPosture Standing => new(0, Math.Tau * 0.2, -Math.Tau * 0.25);
    public static MaydayLegPosture StandingWide => new(0, Math.Tau * 0.1, -Math.Tau * 0.1);
}
