using Generic;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;

namespace Test.Unit.Generic;

public class UnitsNetExtensionsTests
{
    readonly ITestOutputHelper _testOutputHelper;

    public UnitsNetExtensionsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("0", 0.0, 0.0, true)]
    [InlineData("1", 0.0, 0.01, true)]
    [InlineData("2", 0.0, 0.11, false)]
    [InlineData("3", 0.0, 1.0, true)]
    [InlineData("4", 0.0, 0.99, true)]
    [InlineData("5", 0.0, -0.99, true)]
    [InlineData("6", 0.0, 1.99, true)]
    [InlineData("6", 0.0, 1.89, false)]
    void GivenAngleAndOther_WhenIsAlmostEqualSingleRotation_ThenReturnsExpected(
        string testId,
        double angleValue,
        double otherValue,
        bool expectedIsAlmostEqual)
    {
        _testOutputHelper.WriteLine(testId);
    
        // Given
        var angle = Angle.FromRevolutions(angleValue);
        var other = Angle.FromRevolutions(otherValue);
        
        
        // When
        var actualIsAlmostEqual = UnitsNetExtensions.IsAlmostEqualSingleRotation(angle, other, Angle.FromRevolutions(0.1)); 
        
        // Then
        Assert.Equal(expectedIsAlmostEqual, actualIsAlmostEqual);
    }
}
