using System.Numerics;
using mayday.Maths;
using UnitsNet;

namespace mayday.mayday;

public record MaydayLegPosture : Vec3<Angle>
{
    public MaydayLegPosture(Angle coxa, Angle femur, Angle tibia)
        : base(coxa, femur, tibia) {}

    public MaydayLegPosture(double coxa, double femur, double tibia)
        : base(Angle.FromRevolutions(coxa), Angle.FromRevolutions(femur), Angle.FromRevolutions(tibia)) {}

    public static MaydayLegPosture Neutral => new(0.0, 0.0, 0.0);
    public static MaydayLegPosture Sitting => new(0.0, 0.3, -0.2);
    public static MaydayLegPosture Standing => new(0.0, 0.2, -0.25);
    public static MaydayLegPosture StandingWide => new(0.0, 0.1, -0.1);

    public override string ToString()
    {
        return $"{nameof(MaydayLegPosture)}: Coxa: {X0.Revolutions} rev, Femur: {X1.Revolutions} rev, Tibia: {X2.Revolutions} rev";
    }
}
