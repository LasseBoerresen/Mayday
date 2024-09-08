namespace MaydayDomain;

public record MaydayStructurePosture(
    MaydayLegPosture RF,
    MaydayLegPosture RC,
    MaydayLegPosture RB,
    MaydayLegPosture LF,
    MaydayLegPosture LC,
    MaydayLegPosture LB) 
    : MaydayStructureSet<MaydayLegPosture>(RF, RC, RB, LF, LC, LB)
{
    public static MaydayStructurePosture Neutral => FromSingle(MaydayLegPosture.Neutral);
    public static MaydayStructurePosture Sitting => FromSingle(MaydayLegPosture.Sitting);
    public static MaydayStructurePosture SittingTall => FromSingle(MaydayLegPosture.SittingTall);
    public static MaydayStructurePosture Standing => FromSingle(MaydayLegPosture.Standing);
    public static MaydayStructurePosture StandingHigh => FromSingle(MaydayLegPosture.StandingHigh);
    public static MaydayStructurePosture StandingWide => FromSingle(MaydayLegPosture.StandingWide);
}
