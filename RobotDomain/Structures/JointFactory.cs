namespace RobotDomain.Structures;

public interface JointFactory
{
    Joint Create(JointId id);
}
