using RobotDomain.Structures;

namespace Dynamixel;

public class DynamixelJointFactory(Adapter adapter) : JointFactory
{
    public Joint Create(JointId id)
    {
        throw new NotImplementedException();
    }
}