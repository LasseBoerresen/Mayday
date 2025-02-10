namespace MaydayDomain.MotionPlanning;

public record LegProperty<T>(MaydayLegId LegId, T Value)
{
    public LegProperty<U> Map<U>(Func<T, U> mapper) => new(LegId, mapper(Value));
}
