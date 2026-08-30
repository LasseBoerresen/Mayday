using Dynamixel;
using LanguageExt;
using MaydayDataAccess;
using MaydayDomain;
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

    static readonly Eff<JointFactory> jointFactoryEff = DynamixelJointFactory
        .Create(cts, TimeProvider)
        .Map(JointFactory (djf) => djf);
        
    static readonly LegPostureByPositionMap legPostureByPositionMap = new LegPostureByPositionMapFileRepo().Load();
    
    static readonly MaydayLegFactory legFactory = jointFactoryEff
        .Map(jf => new MaydayLegFactory(jf, legPostureByPositionMap))
        .RunUnsafe();

    static readonly MaydayStructure structure = new MaydayStructureFactory(legFactory).CreateDefault();
    
    static readonly MaydayMotionPlanner MotionPlanner = new InstantPostureMaydayMotionPlanner(
        structure, 
        TimeProvider);
    
    public static TheoryData<string, LinkName, Transform>
        DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected()
    {
        return TestObjectFactory.DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected();
    }

    /// <summary>
    /// Tests basic movements of legs, on real robot.
    /// </summary>
    [PhysicalRobotTheory]
    [MemberData(nameof(DataFor_GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected))]
    void GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected(
        string testId, LinkName linkName, Transform expectedTransform)
    {
        // Given
        var timeStep = TimeSpan.FromSeconds(1);
        var timedPosture = TimeProvider.ScheduleIn(MaydayLegPosture.Neutral, timeStep);
        
        MotionPlanner.SetPosture(timedPosture);
        Thread.Sleep(timeStep);
        
        // When
        var actualTransform = MotionPlanner.GetTransformsOf(linkName).LF;

        // Then
        AssertTransformEqual(testId, expectedTransform, actualTransform);
    }

    [PhysicalRobotFact(Skip = "So far only a manual test. Needs to be automated.")]
    void DemonstrationOfVerticalTipMovement()
    {
        // Given
        var minZ = Length.FromMeters(-0.22); // -0.22
        var maxZ = Length.FromMeters(0.18); // 0.2
        var deltaZ = Length.FromMeters(0.01);
        var stanceWidth = Length.FromMeters(0.125);
        var timeStep = TimeSpan.FromSeconds(0.125);



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
    }

    /// <summary>
    /// Tests basic inverse kinematics of legs, on real robot.
    /// </summary>
    [PhysicalRobotFact(Skip = "So far only a manual test. Needs to be automated.")]
    void GivenLegsWithTipAtX015_WhenGetTipPosition_ThenReturnsX015()
    {
        // Given
        throw new NotImplementedException();
    
        // When
        // var actualPosition = MotionPlanner.GetPositionsOf(LinkName.Tip).LF;

        // Then
        // AssertXyzEqual(testId: "bla", expectedPosition, actualPosition);
    }
}
