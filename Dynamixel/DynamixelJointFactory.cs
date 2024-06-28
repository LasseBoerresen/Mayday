using RobotDomain.Structures;

namespace Dynamixel;

public class DynamixelJointFactory(Adapter adapter) : JointFactory
{
    public Joint Create(JointId id)
    {
        var joint = new DynamixelJoint(id, adapter);

        joint.Initialize();

        return joint;
    }
}