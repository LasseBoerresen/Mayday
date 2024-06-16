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
    readonly Mock<Structure> _mockStructure = new();
    readonly InstantPostureMaydayMotionPlanner _instantPostureMaydayMotionPlanner;

    public InstantPostureMotionPlannerTestCase()
    {
        
        _instantPostureMaydayMotionPlanner = new InstantPostureMaydayMotionPlanner();
    }

    [Fact]
    public void Given_WhenSetPostureSitting_ThenStructureSetPostureWithSittingPostureImmediately()
    {
        // Given
        
        // When
        _instantPostureMaydayMotionPlanner.SetPosture(MaydayStructurePosture.Sitting);
        
        // Then
        
    }
}