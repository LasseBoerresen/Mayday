using System.Diagnostics;
using Duration = UnitsNet.Duration;

namespace RobotDomain.Time;

public static class PeriodicScheduler
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

    public static Task RunAsync(Action action, Duration duration, CancellationToken ct)
    {
        return Task.Run(() => Run(action, duration, ct), ct);
    }

    public static void Run(Action action, Duration duration, CancellationToken ct)
    {
        var stopWatch = Stopwatch.StartNew();

        while (!ct.IsCancellationRequested)
        {
            stopWatch.Restart();
            
            CallActionWithErrorLogging(action);
            
            Wait(stopWatch, duration);
        }
    }

    static void CallActionWithErrorLogging(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            // Log the error so it's not ignored!
            Console.WriteLine($"Error in periodic task: {ex.Message}");
            // Depending on requirements, you might want to 'break' or 'continue'
        }
    }

    static void Wait(Stopwatch stopWatch, TimeSpan duration)
    {
        if (stopWatch.Elapsed.TotalMilliseconds > duration.TotalMilliseconds)
            LogActionExceededTimeSlot(stopWatch, duration);
        
        while (stopWatch.Elapsed.TotalMilliseconds < duration.Milliseconds)
        {
            // With thread timing set to 1ms, we can afford to yield slightly to prevent 100% CPU usage
            // but only if we have more than 1ms + 0.5ms (buffer) left.
            var timeRemaining = duration - stopWatch.Elapsed;

            Thread.SpinWait(100);
            // if (timeRemaining > MinTimeForSleep)
            //     Thread.Sleep(timeRemaining - SleepBuffer);
            // else
            //     Thread.SpinWait(10); // Busy wait for the last bit of precision
        }
    }

    static void LogActionExceededTimeSlot(Stopwatch stopWatch, TimeSpan nextTick)
    {
        Console.WriteLine(
            $"Action exceeded time slot in {nameof(PeriodicScheduler)} by {stopWatch.Elapsed - nextTick}");
    }
}
