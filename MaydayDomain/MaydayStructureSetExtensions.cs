using MaydayDomain.MotionPlanning;

namespace MaydayDomain;

public static class MaydayStructureSetExtensions
{
    public static MaydayStructureSet<T> ToMaydayStructureSet<T>(
        this IDictionary<MaydayLegId, T> dictionary)
    {
        return MaydayStructureSet<T>.FromLegDict(dictionary);
    }
    
    public static MaydayStructureSet<T> ToMaydayStructureSet<T>(
        this IEnumerable<LegProperty<T>> propertyList)
    {
        return MaydayStructureSet<T>.FromLegProperties(propertyList);
    }
}
