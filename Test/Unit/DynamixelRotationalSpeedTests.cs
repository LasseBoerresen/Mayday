using Dynamixel;
using Xunit;

namespace Test.Unit;

public class DynamixelRotationalSpeedTests
{
    [Fact]
    void Given_WhenDynamixelRotationalSpeedFrom1StepSpeed_ThenReturns1()
    {
        // When
        var rotationalSpeed = DynamixelRotationalSpeed.StepSize;
        var actual = DynamixelRotationalSpeed.FromRotationalSpeed(rotationalSpeed);

        // Then
        Assert.Equal((uint)1, actual.Value);
    }
}
