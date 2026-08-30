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
public class PeriodicSchedulerTests
{
    readonly ITestOutputHelper _testOutputHelper = new TestOutputHelper();
    readonly FakeTimeProvider _fakeTimeProvider = new();
    readonly PeriodicScheduler _periodicScheduler;
    readonly Duration _testGracePeriod = Duration.FromMilliseconds(1);
    readonly Duration _schedulerDuration = Duration.FromSeconds(1);

    public PeriodicSchedulerTests()
    {
        _periodicScheduler = new(_fakeTimeProvider);
        
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
            action: () => counter++,
            _schedulerDuration,
            CancellationToken.None);
        
        Thread.Sleep(_testGracePeriod);
        _fakeTimeProvider.Advance(_schedulerDuration / 2);
        Thread.Sleep(_testGracePeriod);
        
        // Then
        counter.Should().Be(1);
    }

    [Fact]
    void ShouldCallActionTwiceAfterOneDurationIsPassed()
    {
        // Given
        int counter = 0;

        // When 
        _ = _periodicScheduler.RunAsync(
            action: () => counter++,
            _schedulerDuration,
            CancellationToken.None);
        
        Thread.Sleep(_testGracePeriod);
        _fakeTimeProvider.Advance(_schedulerDuration + _testGracePeriod);
        Thread.Sleep(_testGracePeriod);
        
        // Then
        counter.Should().Be(2);
    }
}