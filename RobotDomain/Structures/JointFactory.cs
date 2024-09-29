using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public interface JointFactory
{
    Joint New(Link parent, Link child, Transform transform, JointId id, Side side);
}
