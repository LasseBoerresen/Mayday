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

    public static TheoryData<string, uint, StepAngle> DataFor_GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle()
    {
        return new() 
        {
            {"0",    1, StepAngle.FromRevs(-0.50)},
            {"1", 1024, StepAngle.FromRevs(-0.25)},
            {"2", 2048, StepAngle.FromRevs( 0.00)},
            {"3", 3072, StepAngle.FromRevs( 0.25)},
            {"4", 4095, StepAngle.FromRevs( 0.50)},
        };
    }

    [Theory]
    [MemberData(nameof(DataFor_GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle))]
    void GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle(
        string testId, uint positionAngleSteps, StepAngle expectedAngle)
    {
        _testOutputHelper.WriteLine(testId);
    
        // When
        StepAngle actualAngle = StepAngle.FromPositionStep(positionAngleSteps);
            
        // Then
        var toleranceOfDynamixel = 1/4096.0;
        Assert.Equal(expectedAngle.Value.Value, actualAngle.Value.Value, toleranceOfDynamixel);
    }
}
