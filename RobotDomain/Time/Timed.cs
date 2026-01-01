using Generic;

namespace RobotDomain.Time;

public record Timed<T>(DateTimeOffset ArrivalTime, T Target)
{
    public static Timed<T> Passed(T command) => new(DateTimeOffset.MinValue, command);

    public Timed<TNew> Map<TNew>(Func<T, TNew> mapper) => new(ArrivalTime, mapper(Target));

    public double StepFactor(DateTimeOffset currentTime, TimeSpan timeStep)
    {
        var remainingTime = (ArrivalTime - currentTime).ClampToPositive();

        if (remainingTime == TimeSpan.Zero) 
            return 1.0;
        
        return Math.Clamp(timeStep / remainingTime, min: 0.0, max: 1.0);
    }

    public Timed<T> ExtendWith(TimeSpan t)
    {
        return this with { ArrivalTime = ArrivalTime + t };
    }
}

public static class TimedExtensions
{
    public static IEnumerable<Timed<T>> Sequence<T>(this Timed<IEnumerable<T>> timedEnumerable)
    {
        return timedEnumerable.Target
            .Select(t => new Timed<T>(timedEnumerable.ArrivalTime, t));
    }

    public static Timed<T> ScheduleIn<T>(this TimeProvider timeProvider, T target, TimeSpan arrivalTime)
    {
        return new(timeProvider.GetUtcNow() + arrivalTime, target);
    }
}