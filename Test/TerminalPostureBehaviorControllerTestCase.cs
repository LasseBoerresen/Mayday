using Domain;
using Domain.Behavior;
using Domain.MotionPlanning;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace Test;

[TestSubject(typeof(TerminalPostureBehaviorController))]
public class TerminalPostureBehaviorControllerTestCase
{
    private readonly Mock<MotionPlanner> _mockMotionPlanner = new();
    private readonly TerminalPostureBehaviorController _controller;

    public TerminalPostureBehaviorControllerTestCase()
    {
        _controller = new TerminalPostureBehaviorController(_mockMotionPlanner.Object);        
    }
    
    
    [Fact]
    public void Test_WhenSitCommand_ThenCallsMotionPlannerWithSittingPosture()
    {
        // Given
        
        // When
        _controller.Sit();

        // Then
        _mockMotionPlanner.Verify(mp => mp.SetPosture(MaydayPosture.Sitting), Times.Once);
    }
    
    [Fact]
    public void Test_WhenStandCommand_ThenCallsMotionPlannerWithStandingPosture()
    {
        // Given
        
        // When
        _controller.Stand();

        // Then
        _mockMotionPlanner.Verify(mp => mp.SetPosture(MaydayPosture.Standing), Times.Once);
    }
}