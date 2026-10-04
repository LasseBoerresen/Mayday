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
    readonly RecordingFatalErrorHandler _fatalErrorHandler = new();

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
            _fatalErrorHandler,
            _updatePeriod);
    }

    /// <summary>
    /// The scenario that used to end the whole test host: the bus fails while the driver's loop is running. The
    /// error now reaches the injected handler, and the test sees it as an ordinary assertion.
    /// </summary>
    [Fact]
    void GivenBusReadThrows_WhenDriverRuns_ThenFatalErrorHandlerReceivesTheException()
    {
        // Given
        var failure = new IOException("the bus failed");
        Mock<CommunicationBus> failingBusMock = new();
        failingBusMock
            .Setup(pa => pa.Read(It.IsAny<IEnumerable<Id>>(), It.IsAny<ControlRegister>()))
            .Throws(failure);
        RecordingFatalErrorHandler fatalErrorHandler = new();

        // When
        using PeriodicallyBatchedJointDriver driver = new(
            new Driver(failingBusMock.Object),
            new JointStateCacheDictImpl(),
            new CancellationTokenSource(),
            _timeProvider,
            fatalErrorHandler,
            _updatePeriod);
        
        SpinWait.SpinUntil(() => fatalErrorHandler.Exceptions.Count >= 1, TimeSpan.FromSeconds(10));

        // Then
        Assert.Contains(failure, fatalErrorHandler.Exceptions);
    }

    // The driver runs a periodic update loop from construction. Stop it, or it outlives the test and can fail-fast
    // the whole test host long after this test has finished.
    public void Dispose()
    {
        _JointDriver.Dispose();
    }

    [Fact]
    void GivenInitializedJoint_WhenSetGoalAngle_ThenBatchWriteContainsStepsForThatJoint()
    {
        // Given
        // A goal other than zero: the joint's initial goal is already the center step, so a zero goal could not
        // be told apart from the goal the joint started with.
        var goalAngle = Angle.FromDegrees(30);
        var expectedSteps = StepAngle.ToSteps(goalAngle);
        var id = Id.FromBase(_id);
        _JointDriver.Initialize(_id, RotationDirection.Forward);

        // When
        _JointDriver.SetGoalAngleFor(_id, Timed<Angle>.Passed(goalAngle));

        // Then
        // Goals are written from the periodic loop, so wait for the write instead of verifying immediately. The
        // loop repeats the write every period, hence "eventually", not "once".
        var written = SpinWait.SpinUntil(
            () => GoalPositionBatches().Any(batch => batch.TryGetValue(id, out var steps) && steps == expectedSteps),
            TimeSpan.FromSeconds(1));
        Assert.True(written, $"No goal position batch with step {expectedSteps} for joint {_id} reached the bus.");
    }

    IEnumerable<IReadOnlyDictionary<Id, uint>> GoalPositionBatches()
    {
        return _communicationBusMock.Invocations
            .Where(i => i.Method.Name == nameof(CommunicationBus.Write)
                        && i.Arguments is [IReadOnlyDictionary<Id, uint>, ControlRegister cr]
                        && cr.Equals(ControlRegister.GoalPosition))
            .Select(i => (IReadOnlyDictionary<Id, uint>)i.Arguments[0])
            .ToList();
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
