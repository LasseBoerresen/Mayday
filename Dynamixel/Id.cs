using RobotDomain.Motion;

namespace Dynamixel;

public record Id(int Value) : ActuatorId
{
    public static Id Broadcast => new(254);
    
    public static Id FromBase(ActuatorId id) => (Id)id;
    
    public static implicit operator byte(Id id) => (byte)id.Value;
    
    // Needed so this type, Id, will equal JointId in dictionary keys (and every where else.)
    // protected override Type EqualityContract => typeof(ActuatorId);
    
    // public static implicit operator JointId(Id id) => new(id.Value);
    
};
