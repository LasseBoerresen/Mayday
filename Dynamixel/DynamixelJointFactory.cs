using RobotDomain.Structures;

namespace Dynamixel;

public class DynamixelJointFactory(DynamixelAdapter adapter) : JointFactory
{
    public Joint Create(JointId id)
    {
        throw new NotImplementedException();
    }
}