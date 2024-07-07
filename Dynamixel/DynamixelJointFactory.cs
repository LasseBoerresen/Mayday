using RobotDomain.Structures;

namespace Dynamixel;

public class DynamixelJointFactory(Adapter adapter) : JointFactory
{
    public Joint Create(Link parent, Link child, JointId id)
    {
        var joint = new DynamixelJoint(parent, child, id, adapter);

        joint.Initialize();

        return joint;
    }
}