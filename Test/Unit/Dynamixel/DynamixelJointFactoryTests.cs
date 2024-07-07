using Dynamixel;
using Moq;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;

namespace Test.Dynamixel;

public class DynamixelJointFactoryTests
{
    readonly Mock<Adapter> _mockAdapter;
    readonly DynamixelJointFactory _dynamixelJointFactory;
    readonly Link _parentLink = Link.New;
    readonly Link _childLink = Link.New;

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
        var actualJoint = _dynamixelJointFactory.Create(_parentLink, _childLink, id);
        
        actualJoint.SetAngleGoal(goalAngle);

        // Then
        _mockAdapter.Verify(a => a.SetGoal(id, new(goalAngle)));
    }
    
    [Fact]
    void GivenMockAdapter_WhenCreateJoint_ThenAdapterInitializeCalledWithJointId()
    {
        // Given
        JointId id = new(1);
    
        // When
        _ = _dynamixelJointFactory.Create(_parentLink, _childLink, id);

        // Then
        _mockAdapter.Verify(a => a.Initialize(id));
    }
}
