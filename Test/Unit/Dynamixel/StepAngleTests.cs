using Dynamixel;
using JetBrains.Annotations;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;

namespace Test.Unit.Dynamixel;

/// <remarks>
/// The XL430-W250 reports and accepts positions 0 to 4,095: 4,096 counts per revolution, 0.088 degrees per count,
/// with 2,048 as the center. See the Robotis e-Manual, XL430-W250 control table, Goal and Present Position.
/// </remarks>
[TestSubject(typeof(StepAngle))]
public class StepAngleTests
{
    const double CountsPerRevolution = 4096.0;
    const double Tolerance = 1e-9;

    readonly ITestOutputHelper _testOutputHelper;

    public StepAngleTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static TheoryData<string, uint, Angle> DataFor_GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle()
    {
        return new()
        {
            {"0",       0, Angle.FromRevolutions(-0.50)},
            {"1",    1024, Angle.FromRevolutions(-0.25)},
            {"2",    2048, Angle.FromRevolutions( 0.00)},
            {"3",    3072, Angle.FromRevolutions( 0.25)},
            {"4",    4095, Angle.FromRevolutions( 0.50 - 1 / CountsPerRevolution)},
        };
    }

    [Theory]
    [MemberData(nameof(DataFor_GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle))]
    void GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle(
        string testId, uint positionAngleSteps, Angle expectedAngle)
    {
        _testOutputHelper.WriteLine(testId);

        // When
        Angle actualAngle = StepAngle.ToAngle(positionAngleSteps);

        // Then
        Assert.Equal(expectedAngle.Revolutions, actualAngle.Revolutions, Tolerance);
    }

    [Fact]
    void GivenAdjacentSteps_WhenToAngle_ThenDifferByOneCountOf360DegreesOver4096()
    {
        // When
        var difference = StepAngle.ToAngle(2049) - StepAngle.ToAngle(2048);

        // Then. 360 / 4096 is the 0.088 degrees per count in the Robotis documentation, unrounded.
        Assert.Equal(360.0 / CountsPerRevolution, difference.Degrees, Tolerance);
    }

    [Fact]
    void GivenEveryValidStep_WhenToAngleThenToSteps_ThenReturnsTheSameStep()
    {
        // The iteration is the behavior under test: every position the actuator can report must be writable back,
        // including both ends of the range.
        for (uint step = 0; step <= 4095; step++)
        {
            // When
            var roundTrip = StepAngle.ToSteps(StepAngle.ToAngle(step));

            // Then
            Assert.Equal(step, roundTrip);
        }
    }

    [Theory]
    [InlineData(0.4, 1000u)]
    [InlineData(0.6, 1001u)]
    [InlineData(-0.4, 1000u)]
    [InlineData(-0.6, 999u)]
    void GivenAngleBetweenTwoSteps_WhenToSteps_ThenReturnsTheNearestStep(double stepsAboveStep1000, uint expectedStep)
    {
        // Given
        var angle = StepAngle.ToAngle(1000) + Angle.FromRevolutions(stepsAboveStep1000 / CountsPerRevolution);

        // When
        var steps = StepAngle.ToSteps(angle);

        // Then
        Assert.Equal(expectedStep, steps);
    }

    [Fact]
    void GivenLowestAngle_WhenToSteps_ThenReturnsStep0()
    {
        Assert.Equal(0u, StepAngle.ToSteps(Angle.FromRevolutions(-0.5)));
    }

    [Theory]
    [InlineData(-0.5001)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    void GivenAngleOutsideHalfRevolutionEitherSide_WhenToSteps_ThenThrows(double revolutions)
    {
        Action toSteps = () => StepAngle.ToSteps(Angle.FromRevolutions(revolutions));

        Assert.Throws<ArgumentException>(toSteps);
    }

    [Fact]
    void GivenAngleJustBelowHalfRevolution_WhenToSteps_ThenReturnsHighestStep()
    {
        Assert.Equal(4095u, StepAngle.ToSteps(Angle.FromRevolutions(0.5 - 1 / CountsPerRevolution)));
    }
}
