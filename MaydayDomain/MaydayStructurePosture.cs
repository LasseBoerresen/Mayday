using RobotDomain.Structures;

namespace MaydayDomain;

public record MaydayStructurePosture(
    MaydayLegPosture RF,
    MaydayLegPosture RC,
    MaydayLegPosture RB,
    MaydayLegPosture LF,
    MaydayLegPosture LC,
    MaydayLegPosture LB) : Posture
{
    public static MaydayStructurePosture Neutral => FromLegPosture(MaydayLegPosture.Neutral);
    public static MaydayStructurePosture Sitting => FromLegPosture(MaydayLegPosture.Sitting);
    public static MaydayStructurePosture Standing => FromLegPosture(MaydayLegPosture.Standing);
    public static MaydayStructurePosture StandingHigh => FromLegPosture(MaydayLegPosture.StandingHigh);
    public static MaydayStructurePosture StandingWide => FromLegPosture(MaydayLegPosture.StandingWide);

    public IDictionary<MaydayLegId, MaydayLegPosture> ToLegDict()
    {
        return new Dictionary<MaydayLegId, MaydayLegPosture>
        {
            { MaydayLegId.LeftFront, LF },
            { MaydayLegId.LeftCenter, LC },
            { MaydayLegId.LeftBack, LB },
            { MaydayLegId.RightFront, RF },
            { MaydayLegId.RightCenter, RC },
            { MaydayLegId.RightBack, RB },
        };
    }

    static MaydayStructurePosture FromLegPosture(MaydayLegPosture legPosture)
    {
        return new(legPosture, legPosture, legPosture, legPosture, legPosture, legPosture);
    }

    public override string ToString()
    {
        return $"MaydayPosture:\n\tRF: {RF},\n\tRC: {RC},\n\tRB: {RB},\n\tLF: {LF},\n\tLC: {LC},\n\tLB: {LB}";
    }
}
