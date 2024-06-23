using JetBrains.Annotations;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using Moq;
using RobotDomain.Structures;
using Xunit;

namespace Test;

[TestSubject(typeof(InstantPostureMaydayMotionPlanner))]
public class InstantPostureMotionPlannerTestCase
{
    readonly Mock<MaydayStructure> _mockStructure = new();
    readonly InstantPostureMaydayMotionPlanner _instantPostureMaydayMotionPlanner;

    public InstantPostureMotionPlannerTestCase()
    {
        _instantPostureMaydayMotionPlanner = new(_mockStructure.Object);
    }

    [Fact(Skip = "not sure how the motion planner architecture should be")]
    public void Given_WhenSetPostureSitting_ThenStructureSetPostureWithSittingPostureImmediately()
    {
        // Given
        
        // When
        _instantPostureMaydayMotionPlanner.SetPosture(MaydayStructurePosture.Sitting);
        
        // Then
        
    }
}