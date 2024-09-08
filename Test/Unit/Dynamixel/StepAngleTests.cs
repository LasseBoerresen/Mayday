using Dynamixel;
using JetBrains.Annotations;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;

namespace Test.Unit.Dynamixel;

[TestSubject(typeof(StepAngle))]
public class StepAngleTests
{
    readonly ITestOutputHelper _testOutputHelper;

    public StepAngleTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static TheoryData<string, uint, Angle> DataFor_GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle()
    {
        return new() 
        {
            {"0",    1, Angle.FromRevolutions(-0.50)},
            {"1", 1024, Angle.FromRevolutions(-0.25)},
            {"2", 2048, Angle.FromRevolutions( 0.00)},
            {"3", 3072, Angle.FromRevolutions( 0.25)},
            {"4", 4095, Angle.FromRevolutions( 0.50)},
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
        var toleranceOfDynamixel = 1/4096.0;
        Assert.Equal(expectedAngle.Value, actualAngle.Value, toleranceOfDynamixel);
    }
}
