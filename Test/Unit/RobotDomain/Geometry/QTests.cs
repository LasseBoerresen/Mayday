using JetBrains.Annotations;
using RobotDomain.Geometry;
using UnitsNet;
using UnitsNet.Units;
using Xunit;
using Xunit.Abstractions;
using static Test.Unit.TestObjectFactory;

namespace Test.Unit.RobotDomain.Geometry;

[TestSubject(typeof(Q))]
public class QTests
{
    readonly ITestOutputHelper _testOutputHelper;

    public QTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static TheoryData<string, Rpy, Q> DataFor_GivenRpy_WhenFromRpy_ThenReturnsExpectedQ()
    {
        return new()
        {
            {"0.0", new(0.0, 0.0, 0.0),   new(W: 1.0, X: 0.0, Y: 0.0, Z: 0.0)},
            {"0.1", new(1.0, 1.0, 1.0),   new(W: 1.0, X: 0.0, Y: 0.0, Z: 0.0)},
            
            {"1.1", new(0.5, 0.0, 0.0),   new(W: 0.0, X: 1.0, Y: 0.0, Z: 0.0)},
            {"1.2", new(0.0, 0.5, 0.0),   new(W: 0.0, X: 0.0, Y: 1.0, Z: 0.0)},
            {"1.3", new(0.0, 0.0, 0.5),   new(W: 0.0, X: 0.0, Y: 0.0, Z: 1.0)},
            
            {"2.1", new(0.25, 0.0, 0.0),  new(W: 0.707, X: 0.707, Y: 0.0, Z: 0.0)},
            {"2.2", new(0.0, 0.25, 0.0),  new(W: 0.707, X: 0.0, Y: 0.707, Z: 0.0)},
            {"2.3", new(0.0, 0.0, 0.25),  new(W: 0.707, X: 0.0, Y: 0.0, Z: 0.707)},
            
            {"3.1", new(1.0/6, 0.0, 0.0), new(W: 0.866, X: 0.5, Y: 0.0, Z: 0.0)},
            {"3.2", new(0.0, 1.0/6, 0.0), new(W: 0.866, X: 0.0, Y: 0.5, Z: 0.0)},
            {"3.3", new(0.0, 0.0, 1.0/6), new(W: 0.866, X: 0.0, Y: 0.0, Z: 0.5)},
            
            {"4.1", new(1.0/3, 0.0, 0.0), new(W: 0.5, X: 0.866, Y: 0.0, Z: 0.0)},
            {"4.2", new(0.0, 1.0/3, 0.0), new(W: 0.5, X: 0.0, Y: 0.866, Z: 0.0)},
            {"4.3", new(0.0, 0.0, 1.0/3), new(W: 0.5, X: 0.0, Y: 0.0, Z: 0.866)},
            
            {"5.1", new(0.75, 0.0, 0.0),  new(W: -0.707, X: 0.707, Y: 0.0, Z: 0.0)},
            {"5.2", new(0.0, 0.75, 0.0),  new(W: -0.707, X: 0.0, Y: 0.707, Z: 0.0)},
            {"5.3", new(0.0, 0.0, 0.75),  new(W: -0.707, X: 0.0, Y: 0.0, Z: 0.707)},
        };
    }

    [Theory]
    [MemberData(nameof(DataFor_GivenRpy_WhenFromRpy_ThenReturnsExpectedQ))]
    void GivenRpy_WhenFromRpy_ThenReturnsExpectedQ(string testId, Rpy rpy, Q expectedQ)
    {
        // When
        var actualQ = Q.FromRpy(rpy);

        // Then
        _testOutputHelper.WriteLine(TestMessage(testId, expectedQ, actualQ));
        
        Assert.True(
            expectedQ.IsAlmostEqual(actualQ, precision: Angle.FromRadians(0.001)), 
            TestMessage(testId, expectedQ, actualQ));
    }
    
    
    public static TheoryData<string, Rpy, Rpy, Q> DataFor_GivenTwoRpy_WhenAdd_ThenReturnsExpectedQ()
    {
        return new()
        {
            {"0.0", new(0.0, 0.0, 0.0), new(0.0, 0.0, 0.0), new(W: 1.0, X: 0.0, Y: 0.0, Z: 0.0)},
            {"0.1", new(1.0, 1.0, 1.0), new(1.0, 1.0, 1.0), new(W: 1.0, X: 0.0, Y: 0.0, Z: 0.0)},
            
            {"1.0", new(0.5, 0.0, 0.0), new(0.5, 0.0, 0.0), new(W: 1.0, X: 0.0, Y: 0.0, Z: 0.0)},
            {"1.1", new(0.0, 0.5, 0.0), new(0.0, 0.5, 0.0), new(W: 1.0, X: 0.0, Y: 0.0, Z: 0.0)},
            {"1.2", new(0.0, 0.0, 0.5), new(0.0, 0.0, 0.5), new(W: 1.0, X: 0.0, Y: 0.0, Z: 0.0)},
            
            {"2.0", new(0.5, 0.0, 0.0), new(0.0, 0.0, 0.0), new(W: 0.0, X: 1.0, Y: 0.0, Z: 0.0)},
            {"2.1", new(0.0, 0.5, 0.0), new(0.0, 0.0, 0.0), new(W: 0.0, X: 0.0, Y: 1.0, Z: 0.0)},
            {"2.2", new(0.0, 0.0, 0.5), new(0.0, 0.0, 0.0), new(W: 0.0, X: 0.0, Y: 0.0, Z: 1.0)},
            
            {"3.0", new(0.4, 0.0, 0.0), new(0.1, 0.0, 0.0), new(W: 0.0, X: 1.0, Y: 0.0, Z: 0.0)},
            {"3.1", new(0.0, 0.4, 0.0), new(0.0, 0.1, 0.0), new(W: 0.0, X: 0.0, Y: 1.0, Z: 0.0)},
            {"3.2", new(0.0, 0.0, 0.4), new(0.0, 0.0, 0.1), new(W: 0.0, X: 0.0, Y: 0.0, Z: 1.0)},
            
            {"4.0", new(0.125, 0.0, 0.0), new(0.125, 0.0, 0.0), new(W: 0.707, X: 0.707, Y: 0.0, Z: 0.0)},
            {"4.1", new(0.0, 0.125, 0.0), new(0.0, 0.125, 0.0), new(W: 0.707, X: 0.0, Y: 0.707, Z: 0.0)},
            {"4.2", new(0.0, 0.0, 0.125), new(0.0, 0.0, 0.125), new(W: 0.707, X: 0.0, Y: 0.0, Z: 0.707)},
            
            {"4.0", new(0.375, 0.0, 0.0), new(0.375, 0.0, 0.0), new(W: -0.707, X: 0.707, Y: 0.0, Z: 0.0)},
            {"4.1", new(0.0, 0.375, 0.0), new(0.0, 0.375, 0.0), new(W: -0.707, X: 0.0, Y: 0.707, Z: 0.0)},
            {"4.2", new(0.0, 0.0, 0.375), new(0.0, 0.0, 0.375), new(W: -0.707, X: 0.0, Y: 0.0, Z: 0.707)},
        };
    }
    
    [Theory]
    [MemberData(nameof(DataFor_GivenTwoRpy_WhenAdd_ThenReturnsExpectedQ))]
    void GivenTwoRpy_WhenAdd_ThenReturnsExpectedQ(string testId, Rpy rpyA, Rpy rpyB, Q expectedQ)
    {
        // Given
        
        // When
        var actualQ = Q.FromRpy(rpyA) + Q.FromRpy(rpyB);

        // Then
        _testOutputHelper.WriteLine(TestMessage(testId, expectedQ, actualQ));

        Assert.True(
            expectedQ.IsAlmostEqual(actualQ, precision: Angle.FromRadians(0.001)), 
            TestMessage(testId, expectedQ, actualQ));
    }
}
