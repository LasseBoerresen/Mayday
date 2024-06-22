using Dynamixel;
using JetBrains.Annotations;
using Moq;
using UnitsNet;
using Xunit;

namespace Test.Dynamixel;

[TestSubject(typeof(AdapterSdkImpl))]
public class AdapterSdkImplTests
{
    readonly Mock<PortAdapter> _dynamixelPortAdapterMock;
    readonly AdapterSdkImpl _adapter;
    readonly Id _id;

    AdapterSdkImplTests()
    {
         _dynamixelPortAdapterMock = new();
         _adapter = new(_dynamixelPortAdapterMock.Object);
         _id = new(1);  
    }

    [Fact]
    void Given_WhenSetGoalToZeroAngle_ThenCallsPortAdapterCorrectly()
    {
        // When
        PositionAngle goalAngle = new(Angle.Zero);
        _adapter.SetGoal(_id, goalAngle);

        // Then
        _dynamixelPortAdapterMock.Verify(
            pa => pa.Write(_id, ControlRegister.GoalPosition, PositionAngle.StepCenter), 
            Times.Once);
    }
}
