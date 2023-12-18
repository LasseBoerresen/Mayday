namespace mayday.mayday;

public record MaydayPosture(
    MaydayLegPosture L0,
    MaydayLegPosture L1,
    MaydayLegPosture L2,
    MaydayLegPosture L3,
    MaydayLegPosture L4,
    MaydayLegPosture L5)
{
    public static MaydayPosture Neutral => FromLegPosture(MaydayLegPosture.Neutral);
    public static MaydayPosture Sitting => FromLegPosture(MaydayLegPosture.Sitting);
    public static MaydayPosture Standing => FromLegPosture(MaydayLegPosture.Standing);
    public static MaydayPosture StandingWide => FromLegPosture(MaydayLegPosture.StandingWide);

    private static MaydayPosture FromLegPosture(MaydayLegPosture legPosture)
        => new(legPosture, legPosture, legPosture, legPosture, legPosture, legPosture);
}
