using Moq;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;
using static RobotDomain.Structures.LinkName;

namespace Test.Unit;

public static class TestObjectFactory
{
    static readonly Length translationPrecision = Length.FromMeters(0.01);
    static readonly Angle rotationalPrecision = Angle.FromRevolutions(0.01);
    public static TimeProvider TimeProvider  => Mock.Of<TimeProvider>();

    public static Mock<Joint> CreateMockJoint() => new(
        Transform.Zero,
        RotationDirection.Forward,
        AttachmentOrder.LinkFirst,
        ComponentId.New, 
        Link.New(LinkName.Base), 
        Link.New(Thorax));
    
    public static string TestMessage(string testId, object expected, object actual)
    {
        return $"TestId: {testId}, \nExpected: {expected}\nActual:   {actual}";
    }

    public static void AssertTransformEqual(string testId, Transform expected, Transform actual)
    {
        Assert.True(
            expected.IsAlmostEqual(actual, translationPrecision, rotationalPrecision), 
            TestMessage(testId, expected, actual));
    }
    
    public static void AssertXyzEqual(string testId, Xyz expected, Xyz actual)
    {
        Assert.True(
            expected.IsAlmostEqual(actual, translationPrecision), 
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