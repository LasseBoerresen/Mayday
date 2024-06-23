using Moq;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;

namespace Dynamixel;

public class DynamixelJointFactoryTests
{
    readonly Mock<Adapter> _mockAdapter;
    readonly DynamixelJointFactory _dynamixelJointFactory;

    public DynamixelJointFactoryTests()
    {
        _mockAdapter = new();
        _dynamixelJointFactory = new(_mockAdapter.Object);
    }

    [Fact]
    void GivenAdapter_WhenCreateJointAndCallSetGoal_ThenAdapterSetGoalCalledOnce()
    {
        // Given
        JointId id = new(1);
        var goalAngle = Angle.FromRevolutions(0.42);
    
        // When
        var actualJoint = _dynamixelJointFactory.Create(id);
        
        actualJoint.SetAngleGoal(goalAngle);

        // Then
        _mockAdapter.Verify(a => a.SetGoal(id, new(goalAngle)));
    }
}
