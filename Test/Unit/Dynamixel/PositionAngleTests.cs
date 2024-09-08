using Dynamixel;
using JetBrains.Annotations;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;

namespace Test.Unit.Dynamixel;

[TestSubject(typeof(PositionAngle))]
public class PositionAngleTests
{
    readonly ITestOutputHelper _testOutputHelper;

    public PositionAngleTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static TheoryData<string, uint, PositionAngle> DataFor_GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle()
    {
        return new() 
        {
            {"0",    1, PositionAngle.FromRevs(-0.50)},
            {"1", 1024, PositionAngle.FromRevs(-0.25)},
            {"2", 2048, PositionAngle.FromRevs( 0.00)},
            {"3", 3072, PositionAngle.FromRevs( 0.25)},
            {"4", 4095, PositionAngle.FromRevs( 0.50)},
        };
    }

    [Theory]
    [MemberData(nameof(DataFor_GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle))]
    void GivenPositionSteps_WhenFromPositionStep_ThenReturnsExpectedAngle(
        string testId, uint positionAngleSteps, PositionAngle expectedAngle)
    {
        _testOutputHelper.WriteLine(testId);
    
        // When
        PositionAngle actualAngle = PositionAngle.FromPositionStep(positionAngleSteps);
            
        // Then
        var toleranceOfDynamixel = 1/4096.0;
        Assert.Equal(expectedAngle.Value.Value, actualAngle.Value.Value, toleranceOfDynamixel);
    }
}
