using RobotDomain.Geometry;
using RobotDomain.Physics;
using RobotDomain.Structures;
using UnitsNet;

namespace Test.Unit;

public class FakeJoint(Angle angle) 
    : Joint(
        Transform.Zero, 
        RotationDirection.Forward, 
        AttachmentOrder.LinkFirst, 
        ComponentId.New, 
        Link.New(LinkName.Base), 
        Link.New(LinkName.Thorax))
{
    public override JointState State => JointState.Zero with { Angle = angle };

    public override void SetAngleGoal(Angle goal)
    {
        throw new NotImplementedException();
    }

    protected override Transform Transform => Transform.Zero;
}