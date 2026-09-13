using AwesomeAssertions;
using RobotDomain.Motion;
using RobotDomain.Time;
using Xunit;

namespace Test.Unit.RobotDomain.Motion;

public class LinearStepsMotionPlanTests
{
    [Fact]
    public void GivenPlanWithOneStep__WhenAtTimeAfter__ThenReturnThatOneStep()
    {
        // Given
        var state = Timed<int>.Passed(42);
        var plan = new LinearStepsMotionPlan<int>([state]);

        // When
        var actualState = plan.At(DateTimeOffset.UtcNow);

        // Then
        actualState.Should().Be(state);
    } 
}
