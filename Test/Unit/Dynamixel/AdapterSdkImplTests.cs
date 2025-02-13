using Dynamixel;
using JetBrains.Annotations;
using Moq;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;
using RotationDirection = RobotDomain.Structures.RotationDirection;

namespace Test.Unit.Dynamixel;

[TestSubject(typeof(AdapterSdkImpl))]
public class AdapterSdkImplTests
{
    readonly Mock<PortAdapter> _dynamixelPortAdapterMock;
    readonly AdapterSdkImpl _adapter;
    readonly JointId _id;

    public AdapterSdkImplTests()
    {
         _dynamixelPortAdapterMock = new();
         _dynamixelPortAdapterMock.Setup(pa => pa.Ping(It.IsAny<Id>())).Returns(true);
         _adapter = new(_dynamixelPortAdapterMock.Object);
         _id = new(1);  
    }

    [Fact]
    void Given_WhenSetGoalToZeroAngle_ThenCallsPortAdapterCorrectly()
    {
        // When
        Angle goalAngle = Angle.Zero;
        _adapter.SetGoal(_id, goalAngle);

        // Then
        _dynamixelPortAdapterMock.Verify(
            pa => pa.Write(_id, ControlRegister.GoalPosition, StepAngle.StepCenter), 
            Times.Once);
    }
    
    [Fact]
    void Given_WhenInitialize_ThenCallsPortAdapterTorqueEnableWithValue1()
    {
        // When
        _adapter.Initialize(_id, RotationDirection.Forward);

        // Then
        _dynamixelPortAdapterMock.Verify(
            pa => pa.Write(_id, ControlRegister.TorqueEnable, Convert.ToUInt32(true)), 
            Times.Once);
    }
    
    [Fact]
    void Given_WhenInitialize_ThenCallsPortAdapterWriteVelocityLimit()
    {
        // When
        _adapter.Initialize(_id, RotationDirection.Forward);

        // Then
        _dynamixelPortAdapterMock.Verify(
            pa => pa.Write(_id, ControlRegister.ProfileVelocity, It.IsAny<uint>()), 
            Times.Once);
    }
}
