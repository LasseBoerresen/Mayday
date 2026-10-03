using System.Diagnostics;
using Duration = UnitsNet.Duration;

namespace RobotDomain.Time;

/// <summary>
/// Runs an action periodically on a precise cadence, and hands any exception the action throws to the injected
/// <see cref="FatalErrorHandler"/>.
/// </summary>
/// <param name="timeProvider">The clock the cadence is measured with, so tests can control time.</param>
/// <param name="fatalErrorHandler">
/// Decides what an exception from the action means. Composition roots that drive a real robot pass
/// <see cref="FailFastFatalErrorHandler"/>.
/// </param>
public class PeriodicScheduler(TimeProvider timeProvider, FatalErrorHandler fatalErrorHandler)
{
    static readonly HighResolutionWindowsTimerSetting HighHighResolutionWindowsTimerSetting;
    
    static readonly TimeSpan ThreadWakePeriod = TimeSpan.FromMilliseconds(1.0);
    static readonly TimeSpan SleepBuffer = TimeSpan.FromMilliseconds(0.5);
    static readonly TimeSpan MinTimeForSleep = ThreadWakePeriod + SleepBuffer;

    static PeriodicScheduler()
    {
        HighHighResolutionWindowsTimerSetting = HighResolutionWindowsTimerSetting.Instance;
        
        // To ensure accurate timing, set the process priority to high.
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
    }

    /// <summary>
    /// Starts <see cref="Run"/> on a thread-pool thread. The returned task completes when the loop ends.
    /// </summary>
    public Task RunAsync(Action action, Duration duration, CancellationToken ct)
    {
        return Task.Run(() => Run(action, duration, ct), ct);
    }

    /// <summary>
    /// Calls <paramref name="action"/> once per <paramref name="duration"/> until <paramref name="ct"/> is
    /// cancelled. Cancellation is only checked between periods.
    /// </summary>
    /// <remarks>
    /// If the action throws, the exception goes to the <see cref="FatalErrorHandler"/> and, should the handler
    /// return, the loop ends instead of repeating the failure every period. A handler used on a real robot ends the
    /// process and never returns.
    /// </remarks>
    public void Run(Action action, Duration duration, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var startTime = timeProvider.GetUtcNow();
            
            if (!TryRunAction(action))
                return;
            
            Wait(startTime, duration);
        }
    }

    /// <returns>True if the action completed; false if it threw and the error was handed to the handler.</returns>
    bool TryRunAction(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            fatalErrorHandler.Handle(ex);
            return false;
        }
    }

    void Wait(DateTimeOffset startTime, TimeSpan duration)
    {
        var now = timeProvider.GetUtcNow();
        var elapsed = now - startTime;
        
        if (elapsed > duration)
            LogActionExceededTimeSlot(elapsed, duration);
        
        while (elapsed < duration)
        {
            // With thread timing set to 1ms, we can afford to yield slightly to prevent 100% CPU usage
            // but only if we have more than 1ms + 0.5ms (buffer) left.
            var timeRemaining = duration - elapsed;

            Thread.SpinWait(iterations: 100);
            now = timeProvider.GetUtcNow();
            elapsed = now - startTime;
            
            // if (timeRemaining > MinTimeForSleep)
            //     Thread.Sleep(timeRemaining - SleepBuffer);
            // else
            //     Thread.SpinWait(10); // Busy wait for the last bit of precision
        }
    }

    static void LogActionExceededTimeSlot(TimeSpan elapsedDuration, TimeSpan targetDuration)
    {
        Console.WriteLine(
            $"Action exceeded time slot of {targetDuration} "
            + $"in {nameof(PeriodicScheduler)} by {elapsedDuration - targetDuration}");
    }
}
