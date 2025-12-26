using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain;

public class EchoJointFactory : JointFactory
{
    public Joint New(
        Link parent, 
        Link child, 
        Transform passiveTransform, 
        JointId id, 
        RotationDirection rotationDirection,
        AttachmentOrder attachmentOrder)
    {
        return new EchoJoint(
            passiveTransform, 
            rotationDirection,
            attachmentOrder,
            ComponentId.New,
            parent,
            child);
    }
}