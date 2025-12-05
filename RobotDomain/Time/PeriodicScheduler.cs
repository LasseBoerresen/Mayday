using UnitsNet;

namespace RobotDomain.Time;

public class PeriodicScheduler
{
    readonly TimeProvider _timeProvider;
    public readonly Duration Duration;

    public PeriodicScheduler(TimeProvider timeProvider, Duration duration)
    {
        _timeProvider = timeProvider;
        Duration = duration;
    }

    public void Run(Action action, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
            action();
    }

    public void WaitForNext()
    {
        // TODO WIP actually calculate how much time to wait, in order to not wait too long. 
        // _timeProvider.GetUtcNow() _duration.
        Thread.Sleep(TimeSpan.FromMicroseconds(Duration.Microseconds));
    }

    public DateTimeOffset CurrentTimeStamp => _timeProvider.GetUtcNow();

}
