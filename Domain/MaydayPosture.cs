using Domain.Structures;

namespace Domain;

public record MaydayPosture(
    MaydayLegPosture RF,
    MaydayLegPosture RC,
    MaydayLegPosture RB,
    MaydayLegPosture LF,
    MaydayLegPosture LC,
    MaydayLegPosture LB) : Posture
{
    public static MaydayPosture Neutral => FromLegPosture(MaydayLegPosture.Neutral);
    public static MaydayPosture Sitting => FromLegPosture(MaydayLegPosture.Sitting);
    public static MaydayPosture Standing => FromLegPosture(MaydayLegPosture.Standing);
    public static MaydayPosture StandingWide => FromLegPosture(MaydayLegPosture.StandingWide);

    private static MaydayPosture FromLegPosture(MaydayLegPosture legPosture)
    {
        return new MaydayPosture(legPosture, legPosture, legPosture, legPosture, legPosture, legPosture);
    }

    public override string ToString()
    {
        return $"MaydayPosture:\n\tRF: {RF},\n\tRC: {RC},\n\tRB: {RB},\n\tLF: {LF},\n\tLC: {LC},\n\tLB: {LB}";
    }
}
