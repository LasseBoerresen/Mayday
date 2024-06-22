using RobotDomain.Structures;

namespace Dynamixel;

public record Id(int Value) : JointId(Value)
{
    public static Id Broadcast => new(254);
};
