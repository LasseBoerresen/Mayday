using System.Diagnostics;
using System.Diagnostics.Metrics;
using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Time.Testing;
using RobotDomain.Time;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.Unit.RobotDomain.Time;

[TestSubject(typeof(PeriodicScheduler))]
public class PeriodicSchedulerTests : IDisposable
{
    static readonly TimeSpan MaxWaitForScheduler = TimeSpan.FromSeconds(10);

    readonly ITestOutputHelper _testOutputHelper = new TestOutputHelper();
    readonly FakeTimeProvider _fakeTimeProvider = new();
    readonly CancellationTokenSource _cancellationTokenSource = new();
    readonly PeriodicScheduler _periodicScheduler;
    readonly Duration _testGracePeriod = Duration.FromMilliseconds(1);
    readonly Duration _schedulerDuration = Duration.FromSeconds(1);

    public PeriodicSchedulerTests()
    {
        _periodicScheduler = new(_fakeTimeProvider);

    }

    /// <summary>
    /// Stops the scheduler loop. The loop only checks for cancellation between periods, and a fake clock never
    /// advances by itself, so cancelling is not enough: advance the clock past the current period as well.
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _fakeTimeProvider.Advance(_schedulerDuration * 2);
    }

    [Fact(Skip = "Manual test so far, needs to be automated with assertions")]
    void ShouldBeWithin1PercentOfDuration()
    {
        // Given
        var period = TimeSpan.FromMilliseconds(1);
        var ctSource = new CancellationTokenSource();
        var testStopwatch = Stopwatch.StartNew();
        var periodStopwatch = Stopwatch.StartNew();

        // When
        _ = _periodicScheduler.RunAsync(
            () =>
            {
                _testOutputHelper.WriteLine($"{periodStopwatch.Elapsed} should be {period}");
                periodStopwatch.Restart();
            },
            period,
            ctSource.Token);

        while (testStopwatch.Elapsed < period * 100)
            Thread.SpinWait(10);

        ctSource.Cancel();

        // Then

    }

    [Fact]
    void ShouldCallOnceActionIfTimeHasNotAdvancedEnough()
    {
        // Given
        int counter = 0;

        // When
        _ = _periodicScheduler.RunAsync(
            action: () => Interlocked.Increment(ref counter),
            _schedulerDuration,
            _cancellationTokenSource.Token);

        // The loop starts on a thread-pool thread, which a busy machine may delay far beyond any fixed sleep.
        WaitUntil(() => Volatile.Read(ref counter) >= 1);
        _fakeTimeProvider.Advance(_schedulerDuration / 2);

        // A second call would have to happen right away; give it a moment to show up. A correct scheduler never fails
        // here, however slow the machine is.
        Thread.Sleep(_testGracePeriod.ToTimeSpan());

        // Then
        Volatile.Read(ref counter).Should().Be(1);
    }

    [Fact]
    void ShouldCallActionTwiceAfterOneDurationIsPassed()
    {
        // Given
        int counter = 0;

        // When
        _ = _periodicScheduler.RunAsync(
            action: () => Interlocked.Increment(ref counter),
            _schedulerDuration,
            _cancellationTokenSource.Token);

        WaitUntil(() => Volatile.Read(ref counter) >= 1);
        _fakeTimeProvider.Advance(_schedulerDuration + _testGracePeriod);
        WaitUntil(() => Volatile.Read(ref counter) >= 2);

        // Then
        Volatile.Read(ref counter).Should().Be(2);
    }

    static void WaitUntil(Func<bool> condition)
    {
        SpinWait.SpinUntil(condition, MaxWaitForScheduler);
    }
}
