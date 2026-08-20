using AwesomeAssertions;
using EllieMain;
using EllieMain.Behaviors;
using EllieMain.MotionPlanning;
using Generic.System;
using Moq;
using RobotDomain.Motion;
using RobotDomain.Structures;
using Test.Utilities;
using UnitsNet;

namespace EllieMainTests.EndToEnd;

/// <summary>
/// Tests which drive the main high level operations of ellie, only mocking
/// that which cannot feasibly be run without the real robot connected. 
/// </summary>
public class EllieTests
{
    readonly Terminal _terminal = new TestTerminal([]);
    readonly Mock<ActuatorDriver> _actuatorDriverMock = new();

    EllieFactory EllieFactory
    {
        get
        {
            TimeProvider timeProvider = TimeProvider.System;
            CancellationTokenSource cts = new();
            PeriodicallyBatchedWheelDriver wheelDriver = new(_actuatorDriverMock.Object);
            var motionPlanner = new ArticulatedSteeringEllieMotionPlanner(wheelDriver);
            
            var behaviorController = new TerminalMovementBehaviorController(
                motionPlanner, 
                _terminal, 
                timeProvider, 
                cts.Token);
            
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
    public async Task WhenStartEllie__ThenAllWheelsAreInitializedOnce()
    {
        // Given
        var ellie = EllieFactory.CreateDefault();

        // When 
        await ellie.StartWaitActWaitStopWait(waitTime: TimeSpan.FromSeconds(1));
        
        // Then
        VerifyWheelInit(WheelId.FrontLeft, RotationDirection.Reverse);
        VerifyWheelInit(WheelId.FrontRight, RotationDirection.Forward);

        void VerifyWheelInit(WheelId wheelId, RotationDirection rotationDirection)
        {
            _actuatorDriverMock.Verify(
                ad => ad.Initialize(wheelId, rotationDirection),
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
    public async Task GivenNewlyStartedEllie__WhenSendAccelerateForwardMovementCommand__ThenAllWheelsSetToNonZeroForwardMotion()
    {
        // Given
        var ellie = EllieFactory.CreateDefault();
        await ellie.StartWaitStop(waitTime: TimeSpan.FromSeconds(1));  
        
        // When
        _terminal.WriteLine(nameof(MovementCommand.AccelerateForward));
        
        // Then
        _actuatorDriverMock.Verify(
            ad => ad.RotateAt(
                WheelId.FrontLeft,
                It.Is<RotationalSpeed>(rs => rs > RotationalSpeed.Zero)), 
            Times.AtLeastOnce);
            
        _actuatorDriverMock.Verify(
            ad => ad.RotateAt(
                    WheelId.FrontRight,
                    It.Is<RotationalSpeed>(rs => rs > RotationalSpeed.Zero)), 
            Times.AtLeastOnce);
    }
}
