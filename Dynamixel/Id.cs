using RobotDomain.Motion;

namespace Dynamixel;

public record Id : ActuatorId
{
    public int Value { get; init; }
    
    public Id(int Value)
    {
        Validate(Value);
        this.Value = Value;
    }

    public static Id Broadcast => new(254);

    static void Validate(int id)
    {
        if (id is < 0 or > 254)
            throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be between 0 and 254");
    }

    public static Id FromBase(ActuatorId id) => new(id.Value);
    
    public static implicit operator byte(Id id) => (byte)id.Value;
    
    // Needed so this type, Id, will equal JointId in dictionary keys (and every where else.)
    // protected override Type EqualityContract => typeof(ActuatorId);
    
    // public static implicit operator JointId(Id id) => new(id.Value);

    
};
