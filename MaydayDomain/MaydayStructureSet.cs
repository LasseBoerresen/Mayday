using System.Collections;
using Generic;
using MaydayDomain.MotionPlanning;
using static MaydayDomain.MaydayLegId;

namespace MaydayDomain;

public record MaydayStructureSet<T>(T RF, T RC, T RB, T LF, T LC, T LB) : IEnumerable<T>
{
    public static MaydayStructureSet<T> FromEnumerable(IEnumerable<T> valuesPerleg)
    {
        var valuesPerLegList = valuesPerleg.ToList();
    
        return new(
            RF: valuesPerLegList[0],
            RC: valuesPerLegList[1],
            RB: valuesPerLegList[2],
            LF: valuesPerLegList[3],
            LC: valuesPerLegList[4],
            LB: valuesPerLegList[5]);
    }

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

    public static MaydayStructureSet<T> FromLegProperties(
        IEnumerable<LegProperty<T>> properties)
    {
        var propertyList = properties.ToList();
    
        return new(
            RF: propertyList[0].Value,
            RC: propertyList[1].Value,
            RB: propertyList[2].Value,
            LF: propertyList[3].Value,
            LC: propertyList[4].Value,
            LB: propertyList[5].Value);
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

    public IEnumerable<LegProperty<T>> ToLegProperties()
    {
        return
        [
            new(LeftFront, LF),
            new(LeftCenter, LC),
            new(LeftBack, LB),
            new(RightFront, RF),
            new(RightCenter, RC),
            new(RightBack, RB)
        ];
    }

    public MaydayStructureSet<U> Map<U>(Func<T, U> mapper)
    {
        return MaydayStructureSet<U>.FromLegDict(ToLegDict().MapValue(mapper));
    }

    public MaydayStructureSet<U> Map<U>(Func<LegProperty<T>, LegProperty<U>> mapper)
    {
        return ToLegProperties()
            .Map(mapper)
            .ToMaydayStructureSet();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ToLegProperties().Select(lp => lp.Value).GetEnumerator();
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

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
};