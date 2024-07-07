namespace RobotDomain.Structures;

public interface JointFactory
{
    Joint Create(Link parent, Link child, JointId id);
}
