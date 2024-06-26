using RobotDomain.Structures;

namespace Dynamixel;

public record Id(int Value) : JointId(Value)
{
    public static Id Broadcast => new(254);

    public static Id FromJointId(JointId id)
    {
        return new Id(id.Value);
    }
};
