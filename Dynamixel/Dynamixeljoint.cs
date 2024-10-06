using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class DynamixelJoint : Joint
{
    readonly Transform _passiveTransform;
    readonly JointId _id;
    readonly RotationDirection _rotationDirection;
    readonly AttachmentOrder _attachmentOrder; 
    readonly Adapter _adapter;

    public DynamixelJoint(
        Link parent,
        Link child,
        Transform passiveTransform,
        JointId id,
        RotationDirection rotationDirection,
        AttachmentOrder attachmentOrder,
        Adapter adapter) 
        : base(ComponentId.New, parent, child)
    {
        _passiveTransform = passiveTransform;
        _id = id;
        _rotationDirection = rotationDirection;
        _attachmentOrder = attachmentOrder;
        _adapter = adapter;
    }
    
    public override JointState State => _adapter.GetState(_id); 
    
    public override void SetAngleGoal(Angle goal) => _adapter.SetGoal(_id, goal);

    public void Initialize() => _adapter.Initialize(_id);

    protected override Transform Transform => 
        _attachmentOrder == AttachmentOrder.LinkLast 
            ? ActiveTransform + _passiveTransform
            : _passiveTransform + ActiveTransform;

    Transform ActiveTransform => Transform.FromQ(Q.FromRpy(new(Angle.Zero, Angle.Zero, Angle)));

    Angle Angle => State.Angle * (int)_rotationDirection;
}