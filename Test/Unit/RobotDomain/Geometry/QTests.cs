using System.Numerics;
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
    Length _lengthPrecision = Length.FromMeters(1e-6);

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
            expectedQ.IsOrientationAlmostEqual(actualQ, precision: 0.001), 
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

    [Fact]
    public void GivenThreeAxisAngleQs_WhenAdd_ThenEqualFromRpyQ()
    {
        // Given
        var qZ = Q.FromAxisAngle(new Vector3(0, 0, 1), Angle.FromRevolutions(0.5));
        var qY = Q.FromAxisAngle(new Vector3(0, 1, 0), Angle.FromRevolutions(-0.125));
        var qX = Q.FromAxisAngle(new Vector3(1, 0, 0), Angle.FromRevolutions(0.25));

        var quaternion = qZ + qY + qX;

        Assert.True(quaternion.IsRotationAlmostEqual(Q.FromRpy(new(0.25, -0.125, 0.5))));
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
            expectedQ.IsOrientationAlmostEqual(actualQ, precision: 0.001), 
            TestMessage(testId, expectedQ, actualQ));
    }
    
    [Fact]
    public void Rotate_IdentityQuaternion_ReturnsSameXyz()
    {
        var quaternion = Q.Unit;
        var point = new Xyz(1, 2, 3);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.Equal(point.X, rotatedPoint.X);
        Assert.Equal(point.Y, rotatedPoint.Y);
        Assert.Equal(point.Z, rotatedPoint.Z);
    }

    [Fact]
    public void Rotate_0_25RevAroundZAxis_RotatesPointCorrectly()
    {
        var quaternion = Q.FromAxisAngle(new Vector3(0, 0, 1), Angle.FromRevolutions(0.25));
        var point = new Xyz(1, 0, 0);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.True(rotatedPoint.IsAlmostEqual(new Xyz(0, 1, 0), _lengthPrecision));
    }

    [Fact]
    public void Rotate_0_25RevAroundYAxis_RotatesPointCorrectly()
    {
        var quaternion = Q.FromAxisAngle(new Vector3(0, 1, 0), Angle.FromRevolutions(0.25));
        var point = new Xyz(1, 0, 0);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.True(rotatedPoint.IsAlmostEqual(new Xyz(0, 0, -1), _lengthPrecision));
    }

    [Fact]
    public void Rotate_0_25RevAroundXAxis_RotatesPointCorrectly()
    {
        var quaternion = Q.FromAxisAngle(new Vector3(1, 0, 0), Angle.FromRevolutions(0.25));
        var point = new Xyz(0, 1, 0);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.True(rotatedPoint.IsAlmostEqual(new Xyz(0, 0, 1), _lengthPrecision));
    }

    [Fact]
    public void Rotate_0_5RevAroundZAxis_RotatesXPointCorrectly()
    {
        var quaternion = Q.FromAxisAngle(new Vector3(0, 0, 1), Angle.FromRevolutions(0.5));
        var point = new Xyz(1, 0, 0);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.True(rotatedPoint.IsAlmostEqual(new Xyz(-1, 0, 0), _lengthPrecision));
    }
    
    [Fact]
    public void Rotate_0_5RevAroundZAxis_RotatesPointYCorrectly()
    {
        var quaternion = Q.FromAxisAngle(new Vector3(0, 0, 1), Angle.FromRevolutions(0.5));
        var point = new Xyz(0, 1, 0);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.True(rotatedPoint.IsAlmostEqual(new Xyz(0, -1, 0), _lengthPrecision));
    }
    
    [Fact]
    public void Rotate_0_5RevAroundZAxis_RotatesPointZCorrectly()
    {
        var quaternion = Q.FromAxisAngle(new Vector3(0, 0, 1), Angle.FromRevolutions(0.5));
        var point = new Xyz(0, 0, 1);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.True(rotatedPoint.IsAlmostEqual(new Xyz(0, 0, 1), _lengthPrecision));
    }
    
    [Fact]
    public void Rotate_0_5RevAroundYAxis_RotatesXPointCorrectly()
    {
        var quaternion = Q.FromAxisAngle(new Vector3(0, 1, 0), Angle.FromRevolutions(0.5));
        var point = new Xyz(1, 0, 0);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.True(rotatedPoint.IsAlmostEqual(new Xyz(-1, 0, 0), _lengthPrecision));
    }
    
    [Fact]
    public void Rotate_0_5RevAroundXAxis_RotatesZPointCorrectly()
    {
        var quaternion = Q.FromAxisAngle(new Vector3(1, 0, 0), Angle.FromRevolutions(0.5));
        var point = new Xyz(0, 0, 1);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.True(rotatedPoint.IsAlmostEqual(new Xyz(0, 0, -1), _lengthPrecision));
    }

    [Fact]
    public void Rotate_NonUnitQuaternion_NormalizesBeforeRotation()
    {
        var quaternion = new Q(2, 0, 0, 0); // Non-unit quaternion
        var point = new Xyz(1, 0, 0);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.True(rotatedPoint.IsAlmostEqual(new Xyz(1, 0, 0), _lengthPrecision));
    }
    
    [Fact]
    public void Rotate_0_125RevAroundZAxis_RotatesXPointCorrectly()
    {
        var quaternion = Q.FromAxisAngle(new Vector3(0, 0, 1), Angle.FromRevolutions(0.125));
        var point = new Xyz(1, 0, 0);

        var rotatedPoint = quaternion.Rotate(point);

        Assert.True(rotatedPoint.IsAlmostEqual(new Xyz(0.707, 0.707, 0), Length.FromMeters(1e-3)));
    }
    
    [Fact]
    public void Rotate_Neg1X_1Y_FromTibiaMotorToTibiaCorrectly()
    {
        // Given
        var quaternion = Q.FromRpy(new(0.25, -0.125, 0.5));
        var point = new Xyz(-1, 1, 0);

        // When
        var rotatedXyz = quaternion.Rotate(point);

        // Then
        Xyz expected = new(1.414, 0, 0);
        Assert.True(
            rotatedXyz.IsAlmostEqual(expected, Length.FromMeters(1e-3)), 
            $"Expected: \n{expected}\ngot: \n{rotatedXyz}from the tibia motor to tibia motor");
    }
    
    [Fact]
    public void Rotate1XFromTibiaMotorToTibiaCorrectly()
    {
        // Given
        var quaternion = Q.FromRpy(new(0.25, -0.125, 0.5));
        var point = new Xyz(1, 0, 0);

        // When
        var rotatedXyz = quaternion.Rotate(point);

        // Then
        Xyz expected = new(-0.707, 0, 0.707);
        Assert.True(
            rotatedXyz.IsAlmostEqual(expected, Length.FromMeters(1e-3)), 
            $"Expected: \n{expected}\ngot: \n{rotatedXyz}from the tibia motor to tibia motor");
    }
    
    [Fact]
    public void RotateNeg1XFromTibiaMotorToTibiaCorrectly()
    {
        // Given
        var quaternion = Q.FromRpy(new(0.25, -0.125, 0.5));
        var point = new Xyz(-1, 0, 0);

        // When
        var rotatedXyz = quaternion.Rotate(point);

        // Then
        Xyz expected = new(0.707, 0, -0.707);
        Assert.True(
            rotatedXyz.IsAlmostEqual(expected, Length.FromMeters(1e-3)), 
            $"Expected: \n{expected}\ngot: \n{rotatedXyz}from the tibia motor to tibia motor");
    }
    
    [Fact]
    public void Rotate1YFromTibiaMotorToTibiaCorrectly()
    {
        // Given
        var quaternion = Q.FromRpy(new(0.25, -0.125, 0.5));
        var point = new Xyz(0, 1, 0);

        // When
        var rotatedXyz = quaternion.Rotate(point);

        // Then
        Xyz expected = new(0.707, 0, 0.707);
        Assert.True(
            rotatedXyz.IsAlmostEqual(expected, Length.FromMeters(1e-3)), 
            $"Expected: \n{expected}\ngot: \n{rotatedXyz}from the tibia motor to tibia motor");
    }
}
