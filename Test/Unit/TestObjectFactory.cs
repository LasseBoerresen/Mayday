using Moq;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;

namespace Test.Unit;

public static class TestObjectFactory
{

    public static Mock<Joint> CreateMockJoint() => new (ComponentId.New, Link.New(LinkName.Base), Link.New(LinkName.Thorax));
    
    public static string TestMessage(string testId, object expected, object actual)
    {
        return $"TestId: {testId}, \nExpected: {expected}\nActual:   {actual}";
    }

    public static void AssertTransformationEqual(Pose expected, Pose actual)
    {
        Length translationPrecision = Length.FromMeters(0.000001);
        Angle rotationalPrecision = Angle.FromRevolutions(0.001);
        
        Assert.True(
            expected.IsAlmostEqual(actual, translationPrecision, rotationalPrecision), 
            TestMessage("2", expected, actual));
    }
}