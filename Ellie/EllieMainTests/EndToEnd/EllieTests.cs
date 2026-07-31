using AwesomeAssertions;
using EllieMain;
using EllieMain.Behaviors;
using EllieMain.MotionPlanning;
using Moq;
using RobotDomain.Motion;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace EllieMainTests.EndToEnd;

/// <summary>
/// Tests which drive the main high level operations of ellie, only mocking
/// that which cannot feasibly be run without the real robot connected. 
/// </summary>
public class EllieTests
{
    readonly Mock<AcDriver> wheelDriverMock = new();

    EllieFactory EllieFactory
    {
        get
        {
            CancellationTokenSource cts = new();
            var motionPlanner = new ArticulatedSteeringEllieMotionPlanner(wheelDriverMock.Object);
            var behaviorController = new TerminalMovementBehaviorController(motionPlanner, cts.Token);
            
            return new EllieFactory(behaviorController, cts);
        }
    }

    [Fact]
    public void WhenCreateDefaultEllie__ThenIsNotNull()
    {
        // When 
        var actualEllie = EllieFactory.CreateDefault(); 
        
        // Then
        actualEllie.Should().NotBeNull();
    }
    
    [Fact]
    public void WhenStartEllie__ThenAllWheelsAreInitializedOnce()
    {
        // Given
        var ellie = EllieFactory.CreateDefault();

        // When 
        ellie.Start();
        Thread.Sleep(TimeSpan.FromSeconds(1));
        ellie.Stop();

        // Then
        VerifyWheelInit(WheelId.FrontLeft, RotationDirection.Reverse);
        VerifyWheelInit(WheelId.FrontRight, RotationDirection.Forward);

        void VerifyWheelInit(WheelId wheelId, RotationDirection rotationDirection)
        {
            wheelDriverMock.Verify(
                wd => wd.Initialize(wheelId, rotationDirection),
                Times.Once);
        }
    }

    // [Fact]
    // public void WhenStartEllie__ThenArticulationJointIsInitializedOnce()
    // {
    //     // Given
    //     var ellie = EllieFactory.CreateDefault();
    //
    //     // When 
    //     ellie.Start();
    //     Thread.Sleep(TimeSpan.FromSeconds(1));
    //
    //     // Then
    //     wheelControllerMock.Verify(
    //         wc => wc.Initialize(
    //             It.Is<WheelId>(id => id == wheelId),
    //             It.IsAny<RobotDomain.Structures.RotationDirection>()),
    //         Times.Once);
    // }
    
    [Fact]
    public void GivenNewlyStartedEllie__WhenSendAccelerateForwardMovementCommand__ThenAllWheelsSetToNonZeroForwardMotion()
    {
        // Given
        var ellie = EllieFactory.CreateDefault();
        ellie.Start();  
        
        // When
        
        
        // Then
        ActuatorDriver.Verify(wd => wd.RotateAt(WheelId.FrontLeft, It.Is<Timed<RotationalSpeed>>(rs => rs > RotationalSpeed.Zero)), Times.AtLeastOnce);
    }
}