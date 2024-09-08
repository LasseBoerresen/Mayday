using static MaydayDomain.MaydayLegId;

namespace MaydayDomain;

public record MaydayStructureSet<T>(T RF, T RC, T RB, T LF, T LC, T LB)
{
    public static MaydayStructureSet<T> FromLegDict(IDictionary<MaydayLegId, T> legDict)
    {
        return new(
            RF: legDict[RightFront],
            RC: legDict[RightCenter],
            RB: legDict[RightBack],
            LF: legDict[LeftFront],
            LC: legDict[LeftCenter],
            LB: legDict[LeftBack]);
    }

    public IDictionary<MaydayLegId, T> ToLegDict()
    {
        return new Dictionary<MaydayLegId, T>
        {
            { LeftFront, LF },
            { LeftCenter, LC },
            { LeftBack, LB },
            { RightFront, RF },
            { RightCenter, RC },
            { RightBack, RB },
        };
    }

    protected static MaydayStructurePosture FromSingle(MaydayLegPosture legPosture)
    {
        return new(legPosture, legPosture, legPosture, legPosture, legPosture, legPosture);
    }

    public override string ToString()
    {
        return $"{GetType().Name}:\n" 
            + $"\tRF: {RF},\n" 
            + $"\tRC: {RC},\n" 
            + $"\tRB: {RB},\n" 
            + $"\tLF: {LF},\n" 
            + $"\tLC: {LC},\n" 
            + $"\tLB: {LB}";
    }
};
