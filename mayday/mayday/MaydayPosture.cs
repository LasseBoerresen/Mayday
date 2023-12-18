using mayday.Structures;

namespace mayday.mayday;

public record MaydayPosture(
    MaydayLegPosture RF,
    MaydayLegPosture RC,
    MaydayLegPosture RB,
    MaydayLegPosture LF,
    MaydayLegPosture LC,
    MaydayLegPosture LB)
{
    public static MaydayPosture Neutral => FromLegPosture(MaydayLegPosture.Neutral);
    public static MaydayPosture Sitting => FromLegPosture(MaydayLegPosture.Sitting);
    public static MaydayPosture Standing => FromLegPosture(MaydayLegPosture.Standing);
    public static MaydayPosture StandingWide => FromLegPosture(MaydayLegPosture.StandingWide);

    private static MaydayPosture FromLegPosture(MaydayLegPosture legPosture)
    {
        return new(legPosture, legPosture, legPosture, legPosture, legPosture, legPosture);
    }

    public override string ToString()
    {
        return $"MaydayPosture:\n\tRF: {RF},\n\tRC: {RC},\n\tRB: {RB},\n\tLF: {LF},\n\tLC: {LC},\n\tLB: {LB}";
    }
}
