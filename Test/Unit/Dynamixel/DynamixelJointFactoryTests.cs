using Dynamixel;
using Moq;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;
using static Dynamixel.RotationDirection;

namespace Test.Unit.Dynamixel;

public class DynamixelJointFactoryTests
{
    readonly Mock<Adapter> _mockAdapter;
    readonly DynamixelJointFactory _dynamixelJointFactory;
    readonly Link _parentLink = Link.New(LinkName.Base);
    readonly Link _childLink = Link.New(LinkName.Thorax);

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
        var actualJoint = _dynamixelJointFactory.New(_parentLink, _childLink, Transform.Zero, id, Forward);
        
        actualJoint.SetAngleGoal(goalAngle);

        // Then
        _mockAdapter.Verify(a => a.SetGoal(id, goalAngle));
    }
    
    [Fact]
    void GivenMockAdapter_WhenCreateJoint_ThenAdapterInitializeCalledWithJointId()
    {
        // Given
        JointId id = new(1);
    
        // When
        _ = _dynamixelJointFactory.New(_parentLink, _childLink, Transform.Zero, id, Forward);

        // Then
        _mockAdapter.Verify(a => a.Initialize(id));
    }
}
