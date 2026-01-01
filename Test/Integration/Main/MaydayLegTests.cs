using LanguageExt;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;
using Test.Unit;
using Test.Utilities;
using Xunit;
using static Test.Unit.TestObjectFactory;
using Length = UnitsNet.Length;

namespace Test.Integration.Main;

public class MaydayLegTests
{
    static readonly TimeProvider TimeProvider = TimeProvider.System;
    static readonly MaydayMotionPlanner MotionPlanner = InstantPostureMaydayMotionPlanner
        .Create(new CancellationTokenSource(), TimeProvider)
        .RunUnsafe();

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
        MotionPlanner.SetPosture(TimeProvider.ScheduleIn(MaydayLegPosture.Neutral, timeStep));
        Thread.Sleep(timeStep);
        
        // When
        var actualTransform = MotionPlanner.GetTransformsOf(linkName).LF;

        // Then
        AssertTransformEqual(testId, expectedTransform, actualTransform);
    }
    
    /// <summary>
    /// Tests basic inverse kinematics of legs, on real robot.
    /// </summary>
    [PhysicalRobotFact]
    void GivenLegsWithTipAtX015_WhenGetTipPosition_ThenReturnsX015()
    {
        // Given
        var minZ = Length.FromMeters(-0.22); // -0.22
        var maxZ = Length.FromMeters(0.18); // 0.2
        var deltaZ = Length.FromMeters(0.001);
        var stanceWidth = Length.FromMeters(0.12);
        var timeStep = TimeSpan.FromSeconds(0.01);

        
        
        for (var z = maxZ; z > minZ; z -= deltaZ)
        {
            var tipPositions = MaydayStructureSet<Xyz>.FromSingle(new Xyz(stanceWidth, Length.Zero, z));
            
            MotionPlanner.SetTipPositionsForLegs(TimeProvider.ScheduleIn(tipPositions, timeStep));
            Thread.Sleep(timeStep);    
        }
        
        for (var x = minZ; x < maxZ; x += deltaZ)
        {
            var tipPositions = MaydayStructureSet<Xyz>.FromSingle(new Xyz(stanceWidth, Length.Zero, x));

            MotionPlanner.SetTipPositionsForLegs(TimeProvider.ScheduleIn(tipPositions, timeStep));
            Thread.Sleep(timeStep);    
        }
        
        // When
        // var actualPosition = MotionPlanner.GetPositionsOf(LinkName.Tip).LF;

        // Then
        // AssertXyzEqual(testId: "bla", expectedPosition, actualPosition); 
    }
}
