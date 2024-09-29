using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public interface JointFactory
{
    Joint Create(Link parent, Link child, Transform transform, JointId id, Side side);
}
