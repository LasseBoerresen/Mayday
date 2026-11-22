using System.Diagnostics;
using AwesomeAssertions;
using JetBrains.Annotations;
using RobotDomain.Time;
using Test.Utilities;
using Xunit;

namespace Test.Unit.RobotDomain.Time;

/// <summary>
/// The scheduler's cadence on the real clock. It runs in <see cref="RealClockCollection"/>, without other tests in
/// parallel: their load stalled the thread by up to 10 ms in the first half-second, so two or more periods missed
/// 1 ms in about 16% of full runs.
/// </summary>
[TestSubject(typeof(PeriodicScheduler))]
[Collection(RealClockCollection.Name)]
public class PeriodicSchedulerCadenceTests
{
    static readonly TimeSpan MaxWaitForScheduler = TimeSpan.FromSeconds(10);

    readonly RecordingFatalErrorHandler _fatalErrorHandler = new();

    /// <summary>
    /// The cadence on the real clock, at 20 ms: the strictest period the robots use (the batched joint driver's).
    /// The mean must hold within 1%, so the cadence neither drifts nor runs at the wrong rate. Single periods may
    /// stall: Windows is not a real-time OS and occasionally holds the thread back by a few milliseconds (with other
    /// tests in parallel, about 1 period in 100, up to 10 ms; 7 runs alone had none over 1 ms), so 99% of periods
    /// must be within 1 ms, not all.
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
}
