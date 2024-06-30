using JetBrains.Annotations;
using ManualBehavior;
using MaydayDomain;
using MaydayDomain.MotionPlanning;
using Moq;
using Xunit;

namespace Test;

[TestSubject(typeof(TerminalPostureBehaviorController))]
public class TerminalPostureBehaviorControllerTestCase
{
    readonly Mock<MaydayMotionPlanner> _mockMotionPlanner = new();
    readonly TerminalPostureBehaviorController _controller;

    public TerminalPostureBehaviorControllerTestCase()
    {
        CancellationTokenSource cancelTokenSource  = new();
        _controller = new(_mockMotionPlanner.Object, cancelTokenSource);        
    }
    
    
    [Fact]
    public void Test_WhenSitCommand_ThenCallsMotionPlannerWithSittingPosture()
    {
        // Given
        
        // When
        _controller.Sit();

        // Then
        _mockMotionPlanner.Verify(mp => mp.SetPosture(MaydayStructurePosture.Sitting), Times.Once);
    }
    
    [Fact]
    public void Test_WhenStandCommand_ThenCallsMotionPlannerWithStandingPosture()
    {
        // Given
        
        // When
        _controller.Stand();

        // Then
        _mockMotionPlanner.Verify(mp => mp.SetPosture(MaydayStructurePosture.Standing), Times.Once);
    }
}