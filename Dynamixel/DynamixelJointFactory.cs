using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace Dynamixel;

public class DynamixelJointFactory(Adapter adapter) : JointFactory
{
    public Joint Create(Link parent, Link child, Transform transform, JointId id)
    {
        var joint = new DynamixelJoint(parent, child, transform, id, adapter);

        joint.Initialize();

        return joint;
    }
}