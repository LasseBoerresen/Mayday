using RobotDomain.Geometry;
using UnitsNet;

namespace RobotDomain.Structures;

public abstract class Joint : Connection
{
    readonly Transform _passiveTransform;
    protected readonly RotationDirection RotationDirection;
    readonly AttachmentOrder _attachmentOrder;
    
    protected Joint(
        Transform passiveTransform,
        RotationDirection rotationDirection,
        AttachmentOrder attachmentOrder,
        ComponentId id, 
        Link parent, 
        Link child) 
        : base(id, parent, child)
    {
        _passiveTransform = passiveTransform;
        RotationDirection = rotationDirection;
        _attachmentOrder = attachmentOrder;
    }
    
    public abstract JointState State { get; }
    public abstract void SetAngleGoal(Angle goal);
    
    protected override Transform Transform => 
        _attachmentOrder == AttachmentOrder.LinkLast 
            ? ActiveTransform + _passiveTransform
            : _passiveTransform + ActiveTransform;

    Transform ActiveTransform => Transform.FromQ(Q.FromRpy(new(Angle.Zero, Angle.Zero, Angle)));

    Angle Angle => State.Angle * (int)RotationDirection * (int)_attachmentOrder;
}
