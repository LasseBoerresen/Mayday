using Dynamixel;
using MaydayDomain;
using Moq;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;
using static Test.Unit.TestObjectFactory;

namespace Test.Unit;

public class MaydayLegTests
{
    readonly ITestOutputHelper _testOutputHelper;
    readonly Mock<JointFactory> _mockJointFactory;

    public MaydayLegTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _mockJointFactory = new();
    }

    public static TheoryData<string, MaydayLegPosture>
        DataForGivenLegWithFakeJointsOfAngle_WhenGetPosture_ThenReturnPostureWithThose()
    {
        return new()
        {
            { "0", new(Angle.FromRevolutions(0.0), Angle.FromRevolutions(0.0), Angle.FromRevolutions(0.0)) },
            { "1", new(Angle.FromRevolutions(0.5), Angle.FromRevolutions(0.5), Angle.FromRevolutions(0.5)) },
            { "2", new(Angle.FromRevolutions(0.1), Angle.FromRevolutions(0.2), Angle.FromRevolutions(0.3)) },
        };
    }

    [Theory]
    [MemberData(nameof(DataForGivenLegWithFakeJointsOfAngle_WhenGetPosture_ThenReturnPostureWithThose))]
    public void GivenLegWithFakeJointsOfAngle_WhenGetPosture_ThenReturnPostureWithThose(
        string testId,
        MaydayLegPosture givenPosture)
    {
        _testOutputHelper.WriteLine($"{nameof(testId)}: {testId}");

        // Given
        var fakeJoints = givenPosture.AsListOfGoalAngles().Select(a => (Connection)new FakeJoint(a)).ToList();
        MaydayLeg leg = new(fakeJoints, []);

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
            { "0", new(Angle.FromRevolutions(0.0), Angle.FromRevolutions(0.0), Angle.FromRevolutions(0.0)) },
            { "1", new(Angle.FromRevolutions(0.5), Angle.FromRevolutions(0.5), Angle.FromRevolutions(0.5)) },
            { "2", new(Angle.FromRevolutions(0.1), Angle.FromRevolutions(0.2), Angle.FromRevolutions(0.3)) },
        };
    }


    [Theory]
    [MemberData(nameof(DataForGivenLeg_WhenSetPosture_ThenSetsJointsWithAngleGoals))]
    public void GivenLeg_WhenSetPosture_ThenSetsJointsWithAngleGoals(
        string testId,
        MaydayLegPosture givenPosture)
    {
        _testOutputHelper.WriteLine($"{nameof(testId)}: {testId}");

        // Given
        List<Mock<Joint>> mockJoints = [CreateMockJoint(), CreateMockJoint(), CreateMockJoint()];
        MaydayLeg leg = new(mockJoints.Select(mj => (Connection)mj.Object).ToList(), []);

        // When
        leg.SetPosture(givenPosture);

        // Then
        mockJoints
            .Zip(givenPosture.AsListOfGoalAngles())
            .ToList()
            .ForEach(pair => pair.First.Verify(j => j.SetAngleGoal(pair.Second), Times.Once));
    }

    [Fact]
    void GivenLegIdLeftFront_WhenCreateLegWithId_ThenCallsJointFactoryWithIds1And2And3()
    {
        // Given
        MaydayLegId legId = MaydayLegId.LeftFront;

        // When
        _ = new MaydayLegFactory(_mockJointFactory.Object).CreateLeg(legId);

        // Then
        VerifyJointFactoryCreateJointId(new(1));
        VerifyJointFactoryCreateJointId(new(2));
        VerifyJointFactoryCreateJointId(new(3));
    }

    [Fact]
    void GivenLegIdRightBack_WhenCreateLegWithId_ThenCallsJointFactoryWithIds16And17And18()
    {
        // Given
        MaydayLegId legId = MaydayLegId.RightBack;

        // When
        _ = new MaydayLegFactory(_mockJointFactory.Object).CreateLeg(legId);

        // Then
        VerifyJointFactoryCreateJointId(new(16));
        VerifyJointFactoryCreateJointId(new(17));
        VerifyJointFactoryCreateJointId(new(18));
    }

    void VerifyJointFactoryCreateJointId(JointId id)
    {
        _mockJointFactory.Verify(jf => 
            jf.Create(
                It.IsAny<Link>(), 
                It.IsAny<Link>(), 
                It.IsAny<Pose>(), 
                id), 
            Times.Once);
    }

    [Fact]
    void GivenMockJointFactory_WhenCreateAll_ThenReturns6Legs()
    {
        // Given

        // When 
        var actualLegsDict = new MaydayLegFactory(_mockJointFactory.Object).CreateAll();

        // Then
        Assert.Equal(6, actualLegsDict.Count);
    }

    [Fact]
    void GivenLegWithJointsAtZero_WhenGetCoxaPose_ThenReturnsPose_X0_Y0_Z0__R0_P0_Y0()
    {
        // Given
        Mock<Adapter> mockDynamixelAdapter = new();
        mockDynamixelAdapter.Setup(a => a.GetState()).Returns(JointState.Zero);
        DynamixelJointFactory jointFactory = new(mockDynamixelAdapter.Object);
        MaydayLegFactory maydayLegFactory = new(jointFactory);
        var leg = maydayLegFactory.CreateLeg(new(Side.Left, SidePosition.Center));

        // When
        var actualOrigin = leg.CoxaPose;

        // Then
        var expectedOrigin = Pose.Zero;
        AssertTransformationEqual(expectedOrigin, actualOrigin);
    }

    [Fact]
    void GivenLegAndCoxaJointStatePositionIs0_WhenGeFemurJointOrigin_ThenReturnsPose_X0d03_Y0_Z0_R0d25_P0_Yn0d25()
    {
        // Given
        Mock<Adapter> mockDynamixelAdapter = new();
        mockDynamixelAdapter.Setup(a => a.GetState()).Returns(JointState.Zero);
        DynamixelJointFactory jointFactory = new(mockDynamixelAdapter.Object);
        MaydayLegFactory maydayLegFactory = new(jointFactory);
        var leg = maydayLegFactory.CreateLeg(new(Side.Left, SidePosition.Center));

        // When
        var actualOrigin = leg.FemurPose;

        // Then
        var expectedOrigin = new Pose(new(0.03, 0, 0.01), Q.FromRpy(new(0.25, 0, 0.25)));

        AssertTransformationEqual(expectedOrigin, actualOrigin);
    }
}
