using RobotDomain.Structures;

namespace Dynamixel;

public record Id(int Value)
{
    public static Id Broadcast => new(254);
    
    public static implicit operator Id(JointId id) => new(id.Value);
};
