using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public interface JointFactory
{
    Joint New(
        Link parent, 
        Link child, 
        Transform passiveTransform, 
        JointId id, 
        RotationDirection rotationDirection,
        AttachmentOrder attachmentOrder);
}
