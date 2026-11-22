using Dynamixel;
using LanguageExt;
using MaydayDomain;
using MaydayDomain.Components;
using MaydayDomain.MotionPlanning;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;
using Test.Unit;
using Test.Utilities;
using Xunit;
using Xunit.Abstractions;
using static Test.Unit.TestObjectFactory;
using Length = UnitsNet.Length;
using TestObjectMother = Test.Integration.MaydayDomain.Base.TestObjectMother;

namespace Test.Integration.Main;

public class MaydayLegTests
{
    readonly ITestOutputHelper _testOutputHelper;

    public MaydayLegTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    static readonly CancellationTokenSource cts = new();
    
    static readonly TimeProvider TimeProvider = TimeProvider.System;

    static readonly HumanVerifier Human = new(new ConsoleWindowHumanPrompt(timeout: TimeSpan.FromMinutes(10)));

    // Physical tests drive a real robot, so they keep the production fail-fast behavior.
    static readonly FatalErrorHandler FatalErrorHandler = new FailFastFatalErrorHandler();

    static readonly Eff<JointFactory> jointFactoryEff = DynamixelJointFactory
        .Create(cts, TimeProvider, FatalErrorHandler)
        .Map(JointFactory (djf) => djf);

    static readonly MaydayLegFactory legFactory = jointFactoryEff
        .Map(jf => new MaydayLegFactory(jf, TestObjectMother.LegPostureByPositionMap))
        .RunUnsafe();

    static readonly MaydayStructure structure = new MaydayStructureFactory(legFactory).CreateDefault();
    
    static readonly MaydayMotionPlanner MotionPlanner = new TrackingMaydayMotionPlanner(
        structure, 
        TimeProvider,
        FatalErrorHandler);
    
    public static TheoryData<string, LinkName, Transform>
        DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected()
    {
        return TestObjectFactory.DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected();
    }

    /// <summary>
    /// Tests basic movements of legs, on real robot. The rows are leg-frame transforms; the planner reports
    /// body-frame ones, so each row is placed at the LF leg's mount on the thorax before comparing.
    /// </summary>
    [PhysicalRobotTheory]
    [MemberData(nameof(DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected))]
    void GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected(
        string testId, LinkName linkName, Transform expectedInLegFrame)
    {
        // Given
        var timeStep = TimeSpan.FromSeconds(1);
        var timedPosture = TimeProvider.ScheduleIn(MaydayLegPosture.Neutral, timeStep);
        
        MotionPlanner.SetPosture(timedPosture);
        Thread.Sleep(timeStep);
        
        // When
        var actualTransform = MotionPlanner.GetTransformsOf(linkName).LF;

        // Then
        var expectedInBodyFrame = Thorax.LeftFrontTransform + expectedInLegFrame;
        AssertTransformEqual(testId, expectedInBodyFrame, actualTransform);
    }

    [ManualRobotFact]
    void GivenTipsAtStanceWidth_WhenMoveAllTipsDownThenUp_ThenAllTipsMoveVertically()
    {
        // Given
        var minZ = Length.FromMeters(-0.22); // -0.22
        var maxZ = Length.FromMeters(0.18); // 0.2
        var deltaZ = Length.FromMeters(0.01);
        var stanceWidth = Length.FromMeters(0.125);
        var timeStep = TimeSpan.FromSeconds(0.125);

        // When
        for (var z = maxZ; z > minZ; z -= deltaZ)
        {
            var tipPositions = MaydayStructureSet<Xyz>.FromSingle(new Xyz(stanceWidth, Length.Zero, z));
            var tipPositionsTimed = TimeProvider.ScheduleIn(tipPositions, timeStep);
            _testOutputHelper.WriteLine($"{TimeProvider.GetUtcNow()}: Setting tip positions to {tipPositionsTimed}");

            MotionPlanner.SetTipPositionsForLegs(tipPositionsTimed);
            _testOutputHelper.WriteLine($"{TimeProvider.GetUtcNow()}: Sleeping");
            Thread.Sleep(timeStep);
        }

        for (var x = minZ; x < maxZ; x += deltaZ)
        {
            var tipPositions = MaydayStructureSet<Xyz>.FromSingle(new Xyz(stanceWidth, Length.Zero, x));
            var tipPositionsTimed = TimeProvider.ScheduleIn(tipPositions, timeStep);
            _testOutputHelper.WriteLine($"{TimeProvider.GetUtcNow()}: Setting tip positions to {tipPositionsTimed}");

            MotionPlanner.SetTipPositionsForLegs(tipPositionsTimed);
            _testOutputHelper.WriteLine($"{TimeProvider.GetUtcNow()}: Sleeping");
            Thread.Sleep(timeStep);
        }

        // Then
        Assert.True(
            Human.Confirms("Did all six leg tips move straight down about 40 cm and then straight back up, without jerks?"),
            "The person watching saw a leg tip that did not move straight down and up.");
    }
}
