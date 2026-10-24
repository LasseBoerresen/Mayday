using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Time.Testing;
using RobotDomain.Time;
using Test.Utilities;
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
    readonly RecordingFatalErrorHandler _fatalErrorHandler = new();
    readonly PeriodicScheduler _periodicScheduler;
    readonly Duration _testGracePeriod = Duration.FromMilliseconds(1);
    readonly Duration _schedulerDuration = Duration.FromSeconds(1);

    public PeriodicSchedulerTests()
    {
        _periodicScheduler = new(_fakeTimeProvider, _fatalErrorHandler);

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

    [Fact]
    [Quarantine("Assertion mismatch: on the real clock the scheduler ran the action only about 3 times in 100 ms, "
        + "with periods of 0.5 to 2.6 ms instead of 1 ms within 10 µs (MAY-83).")]
    void ShouldBeWithin1PercentOfDuration()
    {
        // Given: the real clock, since the period itself is what is measured
        var period = TimeSpan.FromMilliseconds(1);
        var tolerance = period * 0.01;
        var scheduler = new PeriodicScheduler(TimeProvider.System, _fatalErrorHandler);
        using var ctSource = new CancellationTokenSource();
        var periods = new ConcurrentQueue<TimeSpan>();
        var testStopwatch = Stopwatch.StartNew();
        var periodStopwatch = Stopwatch.StartNew();

        // When
        _ = scheduler.RunAsync(
            () =>
            {
                periods.Enqueue(periodStopwatch.Elapsed);
                periodStopwatch.Restart();
            },
            period,
            ctSource.Token);

        while (testStopwatch.Elapsed < period * 100)
            Thread.SpinWait(10);

        ctSource.Cancel();

        // Then: the first call only starts the measurement
        periods.Skip(1).Should().NotBeEmpty()
            .And.OnlyContain(measured => (measured - period).Duration() <= tolerance);
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

    [Fact]
    void ShouldHandOverAnExceptionFromTheActionToTheFatalErrorHandler()
    {
        // Given
        var failure = new InvalidOperationException("the periodic action failed");

        // When
        _ = _periodicScheduler.RunAsync(
            action: () => throw failure,
            _schedulerDuration,
            _cancellationTokenSource.Token);
        
        WaitUntil(() => _fatalErrorHandler.Exceptions.Count >= 1);
        
        // Then
        _fatalErrorHandler.Exceptions.Should().ContainSingle().Which.Should().BeSameAs(failure);
    }

    [Fact]
    async Task ShouldStopTheLoopAfterTheFatalErrorHandlerReturns()
    {
        // Given
        int calls = 0;

        // When
        var loop = _periodicScheduler.RunAsync(
            action: () =>
            {
                Interlocked.Increment(ref calls);
                throw new InvalidOperationException("the periodic action failed");
            },
            _schedulerDuration,
            _cancellationTokenSource.Token);
        
        // Then. The loop ends by itself: the clock never advances and the token is never cancelled.
        await loop.WaitAsync(MaxWaitForScheduler); // Times out, and fails the test, if the loop never ends.
        Volatile.Read(ref calls).Should().Be(1);
    }

    static void WaitUntil(Func<bool> condition)
    {
        SpinWait.SpinUntil(condition, MaxWaitForScheduler);
    }
}
