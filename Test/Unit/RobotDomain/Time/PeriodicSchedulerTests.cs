using System.Diagnostics;
using JetBrains.Annotations;
using RobotDomain.Time;
using Xunit;
using Xunit.Abstractions;

namespace Test.Unit.RobotDomain.Time;

[TestSubject(typeof(PeriodicScheduler))]
public class PeriodicSchedulerTests(ITestOutputHelper testOutputHelper)
{
    [Fact(Skip = "Manual test so far, needs to be automated with assertions")]
    void ShouldBeWithin1PercentOfDuration()
    {
        // Given
        var period = TimeSpan.FromMilliseconds(1);
        var ctSource = new CancellationTokenSource();
        var testStopwatch = Stopwatch.StartNew();
        var periodStopwatch = Stopwatch.StartNew();
        
        
        // When
        _ = PeriodicScheduler.RunAsync(
            () =>
            {
                testOutputHelper.WriteLine($"{periodStopwatch.Elapsed} should be {period}");
                periodStopwatch.Restart();
            },
            period, 
            ctSource.Token);

        while (testStopwatch.Elapsed < period * 100)
            Thread.SpinWait(10);
        
        ctSource.Cancel();
        
        // Then

    }
}