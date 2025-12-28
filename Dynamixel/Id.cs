using RobotDomain.Structures;

namespace Dynamixel;

public record Id(int Value) : JointId(Value)
{
    public static Id Broadcast => new(254);
    
    public static Id FromBase(JointId id) => new(id.Value);
    
    public static implicit operator byte(Id id) => (byte)id.Value;
    
    // public static implicit operator JointId(Id id) => new(id.Value);
    
};
