using Dynamixel;
using JetBrains.Annotations;
using Moq;
using RobotDomain.Motion;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;
using Xunit;
using RotationDirection = RobotDomain.Structures.RotationDirection;

namespace Test.Unit.Dynamixel;

[TestSubject(typeof(PeriodicallyBatchedJointDriver))]
public class PeriodicallyBatchedJointDriverTests
{
    readonly Mock<CommunicationBus> _communicationBusMock = new();
    readonly PeriodicallyBatchedJointDriver _JointDriver;
    readonly JointId _id = new(1);
    readonly TimeProvider _timeProvider = TimeProvider.System;

    public PeriodicallyBatchedJointDriverTests()
    {
        _communicationBusMock.Setup(pa => pa.Ping(It.IsAny<Id>())).Returns(true);
        Driver driver = new(_communicationBusMock.Object);
        
        _JointDriver = new(
            driver,
            new Mock<JointStateCache>().Object, 
            new CancellationTokenSource(),
            _timeProvider);
    }

    // TODO: This test is no longer correct, because portAdapter is no longer called to write single goal angles, but 
    //  all at once. Also, they are written asyncronyously, so really we should only test if it is written within a
    //  certain time frame, like 20ms. 
    [Fact]
    void Given_WhenSetGoalToZeroAngle_ThenCallsCommunicationBusCorrectly()
    {
        // When
        var goalAngle = Timed<Angle>.Passed(Angle.Zero);
        _JointDriver.SetGoalAngleFor(_id, goalAngle);

        // Then
        _communicationBusMock.Verify(
            pa => pa.Write(Id.FromBase(_id), ControlRegister.GoalPosition, StepAngle.StepCenter), 
            Times.Once);
    }
    
    [Fact]
    void Given_WhenInitialize_ThenCallsCommunicationBusTorqueEnableWithValue1()
    {
        // When
        _JointDriver.Initialize(_id, RotationDirection.Forward);

        // Then
        _communicationBusMock.Verify(
            pa => pa.Write(Id.FromBase(_id), ControlRegister.TorqueEnable, Convert.ToUInt32(true)), 
            Times.Once);
    }
    
    [Fact]
    void Given_WhenInitialize_ThenCallsCommunicationBusWriteVelocityLimit()
    {
        // When
        _JointDriver.Initialize(_id, RotationDirection.Forward);

        // Then
        _communicationBusMock.Verify(
            pa => pa.Write(Id.FromBase(_id), ControlRegister.ProfileVelocity, It.IsAny<uint>()), 
            Times.Once);
    }
}
