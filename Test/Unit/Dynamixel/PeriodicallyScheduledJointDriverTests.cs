using Dynamixel;
using JetBrains.Annotations;
using Moq;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;
using Xunit;
using RotationDirection = RobotDomain.Structures.RotationDirection;

namespace Test.Unit.Dynamixel;

[TestSubject(typeof(PeriodicallyScheduledJointDriver))]
public class PeriodicallyScheduledJointDriverTests
{
    readonly Mock<CommunicationBus> _dynamixelPortAdapterMock = new();
    readonly PeriodicallyScheduledJointDriver _JointDriver;
    readonly JointId _id = new(1);
    readonly TimeProvider _timeProvider = TimeProvider.System;

    public PeriodicallyScheduledJointDriverTests()
    {
        _dynamixelPortAdapterMock.Setup(pa => pa.Ping(It.IsAny<Id>())).Returns(true);
         
         _JointDriver = new(
             _dynamixelPortAdapterMock.Object, 
             new Mock<JointStateCache>().Object, 
             new CancellationTokenSource(),
             _timeProvider);
    }

    // TODO: This test is no longer correct, because portAdapter is no longer called to write single goal angles, but 
    //  all at once. Also, they are written asyncronyously, so really we should only test if it is written within a
    //  certain time frame, like 20ms. 
    [Fact]
    void Given_WhenSetGoalToZeroAngle_ThenCallsPortAdapterCorrectly()
    {
        // When
        var goalAngle = Timed<Angle>.Passed(Angle.Zero);
        _JointDriver.SetGoalAngleFor(_id, goalAngle);

        // Then
        _dynamixelPortAdapterMock.Verify(
            pa => pa.Write(Id.FromBase(_id), ControlRegister.GoalPosition, StepAngle.StepCenter), 
            Times.Once);
    }
    
    [Fact]
    void Given_WhenInitialize_ThenCallsPortAdapterTorqueEnableWithValue1()
    {
        // When
        _JointDriver.Initialize(_id, RotationDirection.Forward);

        // Then
        _dynamixelPortAdapterMock.Verify(
            pa => pa.Write(Id.FromBase(_id), ControlRegister.TorqueEnable, Convert.ToUInt32(true)), 
            Times.Once);
    }
    
    [Fact]
    void Given_WhenInitialize_ThenCallsPortAdapterWriteVelocityLimit()
    {
        // When
        _JointDriver.Initialize(_id, RotationDirection.Forward);

        // Then
        _dynamixelPortAdapterMock.Verify(
            pa => pa.Write(Id.FromBase(_id), ControlRegister.ProfileVelocity, It.IsAny<uint>()), 
            Times.Once);
    }
}
