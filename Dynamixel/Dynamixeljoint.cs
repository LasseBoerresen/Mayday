using RobotDomain.Geometry;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class DynamixelJoint : Joint
{
    readonly Transform _passiveTransform;
    readonly JointId _id;
    readonly RobotDomain.Structures.RotationDirection _rotationDirection;
    readonly AttachmentOrder _attachmentOrder; 
    readonly Adapter _adapter;

    public DynamixelJoint(
        Link parent,
        Link child,
        Transform passiveTransform,
        JointId id,
        RobotDomain.Structures.RotationDirection rotationDirection,
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
    
    // TODO: create state proxy, that simply updates at a base frequency,
    //  to decouple queries from communication with dynamixel, and always
    //  just returns the current value.
    //  I could add a "boost" functionality, where whenever cache is hit, we
    //  double the frequency, but it decays on its own. 
    public override JointState State => _adapter.GetState(_id); 
    
    public override void SetAngleGoal(Angle goal) => _adapter.SetGoal(_id, goal);

    public void Initialize() => _adapter.Initialize(_id, _rotationDirection);

    protected override Transform Transform => 
        _attachmentOrder == AttachmentOrder.LinkLast 
            ? ActiveTransform + _passiveTransform
            : _passiveTransform + ActiveTransform;

    Transform ActiveTransform => Transform.FromQ(Q.FromRpy(new(Angle.Zero, Angle.Zero, Angle)));

    Angle Angle => State.Angle * (int)_rotationDirection * (int)_attachmentOrder;
}