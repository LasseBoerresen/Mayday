using Generic;

namespace RobotDomain.Time;

public record Timed<T>(DateTimeOffset ArrivalTime, DateTimeOffset IssueTime, T Target)
{
    public static Timed<T> Passed(T command) => new(DateTimeOffset.MinValue, DateTimeOffset.MinValue, command);

    public Timed<TNew> Map<TNew>(Func<T, TNew> mapper) => new(ArrivalTime, IssueTime, mapper(Target));

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
            .Select(t => new Timed<T>(timedEnumerable.ArrivalTime, timedEnumerable.IssueTime, t));
    }

    public static Timed<T> ScheduleIn<T>(this TimeProvider timeProvider, T target, TimeSpan arrivalTime)
    {
        var now = timeProvider.GetUtcNow();
        
        return new(now + arrivalTime, now, target);
    }
}