using MaydayDomain;
using Moq;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;

namespace Test;

public class MaydayLegTests
{
    readonly ITestOutputHelper _testOutputHelper;

    public MaydayLegTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static TheoryData<string, MaydayLegPosture> DataForGivenLegWithFakeJointsOfAngle_WhenGetPosture_ThenReturnPostureWithThose()
    {
        return new()
        { 
            { "0", new(Angle.FromRevolutions(0.0), Angle.FromRevolutions(0.0), Angle.FromRevolutions(0.0))},
            { "1", new(Angle.FromRevolutions(0.5), Angle.FromRevolutions(0.5), Angle.FromRevolutions(0.5))},
            { "2", new(Angle.FromRevolutions(0.1), Angle.FromRevolutions(0.2), Angle.FromRevolutions(0.3))},
        };
    }
    
    [Theory]
    [MemberData(nameof(DataForGivenLegWithFakeJointsOfAngle_WhenGetPosture_ThenReturnPostureWithThose))]
    public void GivenLegWithFakeJointsOfAngle_WhenGetPosture_ThenReturnPostureWithThose(
        string testId, MaydayLegPosture givenPosture)
    {
        _testOutputHelper.WriteLine($"{nameof(testId)}: {testId}");
        
        // Given
        var fakeJoints = givenPosture.AsEnumerable().Select(a => new FakeJoint(a)); 
        MaydayLeg leg = new(fakeJoints);
        
        // When
        var actual = leg.GetPosture();
        
        // Then
        var expected = givenPosture;
        Assert.Equal(expected, actual);
    }
    
    public static TheoryData<string, MaydayLegPosture> DataForGivenLeg_WhenSetPosture_ThenSetsJointsWithAngleGoals()
    {
        return new()
        { 
            { "0", new(Angle.FromRevolutions(0.0), Angle.FromRevolutions(0.0), Angle.FromRevolutions(0.0))},
            { "1", new(Angle.FromRevolutions(0.5), Angle.FromRevolutions(0.5), Angle.FromRevolutions(0.5))},
            { "2", new(Angle.FromRevolutions(0.1), Angle.FromRevolutions(0.2), Angle.FromRevolutions(0.3))},
        };
    }


    [Theory]
    [MemberData(nameof(DataForGivenLeg_WhenSetPosture_ThenSetsJointsWithAngleGoals))]
    public void GivenLeg_WhenSetPosture_ThenSetsJointsWithAngleGoals(
        string testId, MaydayLegPosture givenPosture)
    {
        _testOutputHelper.WriteLine($"{nameof(testId)}: {testId}");
        
        // Given
        List<Mock<Joint>> mockJoints = [new(), new(), new()];
        MaydayLeg leg = new(mockJoints.Select(mj => mj.Object));
        
        // When
        leg.SetPosture(givenPosture);
        
        // Then
        mockJoints
            .Zip(givenPosture.AsEnumerable())
            .ToList()
            .ForEach(pair => pair.First.Verify(j => j.SetAngleGoal(pair.Second), Times.Once));
    }

    [Fact]
    void GivenLegIdLeftFront_WhenCreateLegWithId_ThenCallsJointFactoryWithIds1And2And3()
    {
        // Given
        MaydayLegId legId = MaydayLegId.LeftFront;
        Mock<JointFactory> mockJointFactory = new();
        
        // When
        _ = MaydayLeg.CreateLeg(legId, mockJointFactory.Object);

        // Then
        mockJointFactory.Verify(jf => jf.Create(new(1)), Times.Once);
        mockJointFactory.Verify(jf => jf.Create(new(2)), Times.Once);
        mockJointFactory.Verify(jf => jf.Create(new(3)), Times.Once);
    }

    [Fact]
    void GivenMockJointFactory_WhenCreateAll_ThenReturns6Legs()
    {
        // Given
        Mock<JointFactory> mockJointFactory = new();
        
        // When 
        var actualLegsDict = MaydayLeg.CreateAll(mockJointFactory.Object);

        // Then
        Assert.Equal(6, actualLegsDict.Count);
    }
}