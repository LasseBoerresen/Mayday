using JetBrains.Annotations;
using RobotDomain.Geometry;
using Xunit;
using Xunit.Abstractions;

namespace Test.Unit.RobotDomain.Geometry;

[TestSubject(typeof(Transform))]
public class TransformTests
{
    readonly ITestOutputHelper _testOutputHelper;

    public TransformTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static TheoryData<string, Transform, Transform, Transform>
        DataFor_GivenTwoTransforms__WhenAdd__ThenReturnsExpectedTransform()
    {
        return new()
        {
            {"0", Transform.Zero, Transform.Zero, Transform.Zero},
            {"1", Transform.FromXyz(new(1, 0, 0)), Transform.Zero, Transform.FromXyz(new(1, 0, 0))},
            {"2", Transform.FromXyz(new(1, 0, 0)), Transform.FromXyz(new(1, 0, 0)), Transform.FromXyz(new(2, 0, 0))},
            {
                "3, quarter yaw: 1x -> 1y", 
                Transform.FromQ(Q.FromRpy(new(0, 0, 0.25))), 
                Transform.FromXyz(new(1, 0, 0)), 
                new Transform(new(0, 1, 0), Q.FromRpy(new(0, 0, 0.25)))},
        };
    }

    [Theory]
    [MemberData(nameof(DataFor_GivenTwoTransforms__WhenAdd__ThenReturnsExpectedTransform))]
    void GivenTwoTransforms__WhenAdd__ThenReturnsExpectedTransform(string testId, Transform a, Transform b, Transform expected)
    {
        _testOutputHelper.WriteLine(testId);
    
        // When
        var actual = a + b;
        
        // Then
        TestObjectFactory.AssertTransformEqual(testId, expected, actual);
    }
}
