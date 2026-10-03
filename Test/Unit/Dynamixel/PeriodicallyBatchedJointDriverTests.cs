using Dynamixel;
using Test.Utilities;
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
public class PeriodicallyBatchedJointDriverTests : IDisposable
{
    readonly Mock<CommunicationBus> _communicationBusMock = new();
    readonly PeriodicallyBatchedJointDriver _JointDriver;
    readonly JointId _id = new(1);
    readonly TimeProvider _timeProvider = TimeProvider.System;
    readonly TimeSpan _updatePeriod = TimeSpan.FromMilliseconds(1);

    public PeriodicallyBatchedJointDriverTests()
    {
        _communicationBusMock.Setup(pa => pa.Ping(It.IsAny<Id>())).Returns(true);

        // The periodic update loop starts in the driver constructor and reads angles every period. An unconfigured
        // mock returns null for this dictionary, which throws on the scheduler thread and fail-fasts the test host.
        _communicationBusMock
            .Setup(pa => pa.Read(It.IsAny<IEnumerable<Id>>(), It.IsAny<ControlRegister>()))
            .Returns(new Dictionary<Id, uint>());

        // Initialize reads the joint's present and goal position. Report the center step, so the joint rests at zero
        // angle instead of at Moq's default of step 0, which is -0.5 revolutions.
        _communicationBusMock
            .Setup(pa => pa.Read(It.IsAny<Id>(), It.IsAny<ControlRegister>()))
            .Returns(StepAngle.StepCenter);

        Driver driver = new(_communicationBusMock.Object);
        JointStateCacheDictImpl jointStateCache = new();
        
        _JointDriver = new PeriodicallyBatchedJointDriver(
            driver,
            jointStateCache, 
            new CancellationTokenSource(),
            _timeProvider,
            _updatePeriod);
    }

    // The driver runs a periodic update loop from construction. Stop it, or it outlives the test and can fail-fast
    // the whole test host long after this test has finished.
    public void Dispose()
    {
        _JointDriver.Dispose();
    }

    // TODO: This test is no longer correct, because portAdapter is no longer called to write single goal angles, but 
    //  all at once. Also, they are written asyncronyously, so really we should only test if it is written within a
    //  certain time frame, like 20ms. 
    [Fact]
    [Quarantine("Stale: expects a single Write, but goals are written in batches and the joint is never initialized (see the TODO in the test).")]
    void Given_WhenSetGoalToZeroAngle_ThenCallsCommunicationBusCorrectly()
    {
        // TODO control time in the PeriodicScheduler to be able to properly test 
    
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
