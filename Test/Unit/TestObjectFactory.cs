using Moq;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;
using static RobotDomain.Structures.LinkName;

namespace Test.Unit;

public static class TestObjectFactory
{

    public static Mock<Joint> CreateMockJoint() => new (ComponentId.New, Link.New(Base), Link.New(Thorax));
    
    public static string TestMessage(string testId, object expected, object actual)
    {
        return $"TestId: {testId}, \nExpected: {expected}\nActual:   {actual}";
    }

    public static void AssertTransformEqual(string testId, Transform expected, Transform actual)
    {
        Length translationPrecision = Length.FromMeters(0.01);
        Angle rotationalPrecision = Angle.FromRevolutions(0.01);
        
        Assert.True(
            expected.IsAlmostEqual(actual, translationPrecision, rotationalPrecision), 
            TestMessage(testId, expected, actual));
    }
    
    public static TheoryData<string, LinkName, Transform>
        DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected()
    {
        return new()
        {
            { "0", CoxaMotor, Transform.Zero},
            { "1", Coxa, Transform.Zero},
            { "2", FemurMotor, new(new(0.033, 0, -0.013), Q.FromRpy(new(-0.25, 0.25, 0 )))},
            { "3", Femur,      new(new(0.033, 0, -0.013), Q.FromRpy(new(0.0, 0.0, 0.0)))},
            { "4", TibiaMotor, new(new(0.115, 0, 0.02), Q.FromRpy(new(0.25, -0.125, 0.5)))},
            { "5", Tibia,      new(new(0.135, 0, 0.02), Q.FromRpy(new(0.0, 0.125 , 0.0)))},
            { "6", Tip,        new(new(0.165, 0, -0.125), Q.FromRpy(new(0.0, 0.29166, 0.0)))}, 
        };
    }
}