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

    /// <summary>
    /// The cadence on the real clock, at 20 ms: the strictest period the robots use (the batched joint driver's).
    /// The mean must hold within 1%, so the cadence neither drifts nor runs at the wrong rate. Single periods may
    /// stall: Windows is not a real-time OS and occasionally holds the thread back by a few milliseconds (measured
    /// about 1 period in 1,500 to 4,500, up to 6.3 ms under load), so 99% of periods must be within 1 ms, not all.
    /// </summary>
    [Fact]
    async Task GivenTheRealClockAnd20MsPeriod_When100PeriodsRun_ThenMeanIsWithin1PercentAnd99PercentAreWithin1Ms()
    {
        // Given
        var period = TimeSpan.FromMilliseconds(20);
        const int periodCount = 100;
        var scheduler = new PeriodicScheduler(TimeProvider.System, _fatalErrorHandler);
        using var ctSource = new CancellationTokenSource();

        // One timestamp per call, into a preallocated array: recording a call must not delay the next one.
        var callTimestamps = new long[periodCount + 1];
        var calls = 0;
        void RecordCall()
        {
            var call = Volatile.Read(ref calls);
            if (call < callTimestamps.Length)
                callTimestamps[call] = Stopwatch.GetTimestamp();
            Volatile.Write(ref calls, call + 1);
        }

        // Compile the action before measuring: compiling inside the first calls makes their periods read short.
        RecordCall();
        Volatile.Write(ref calls, 0);

        // When
        var loop = scheduler.RunAsync(RecordCall, period, ctSource.Token);
        SpinWait.SpinUntil(() => Volatile.Read(ref calls) > periodCount, MaxWaitForScheduler);
        ctSource.Cancel();
        await loop.WaitAsync(MaxWaitForScheduler);

        // Then
        Volatile.Read(ref calls).Should().BeGreaterThan(periodCount);
        var periods = Enumerable.Range(1, periodCount)
            .Select(i => Stopwatch.GetElapsedTime(callTimestamps[i - 1], callTimestamps[i]))
            .ToList();
        var meanPeriod = TimeSpan.FromTicks((long)periods.Average(measured => measured.Ticks));
        meanPeriod.Should().BeCloseTo(period, period * 0.01);
        periods.Count(measured => (measured - period).Duration() <= TimeSpan.FromMilliseconds(1))
            .Should().BeGreaterThanOrEqualTo(99, "a single stall of the thread is tolerated, regular jitter is not");
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
