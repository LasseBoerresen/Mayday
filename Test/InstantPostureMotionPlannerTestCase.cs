using Domain;
using Domain.MotionPlanning;
using Domain.Structures;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace Test;

[TestSubject(typeof(InstantPostureMotionPlanner))]
public class InstantPostureMotionPlannerTestCase
{
    readonly Mock<Structure> _mockStructure = new();
    readonly InstantPostureMotionPlanner _instantPostureMotionPlanner;

    public InstantPostureMotionPlannerTestCase()
    {
        
        _instantPostureMotionPlanner = new InstantPostureMotionPlanner();
    }

    [Fact]
    public void Given_WhenSetPostureSitting_ThenStructureSetPostureWithSittingPostureImmediately()
    {
        // Given
        
        // When
        _instantPostureMotionPlanner.SetPosture(MaydayPosture.Sitting);
        
        // Then
        
    }
}