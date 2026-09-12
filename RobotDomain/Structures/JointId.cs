using RobotDomain.Motion;

namespace RobotDomain.Structures;

public record JointId(int Value) : ActuatorId
{
    public static JointId FromBase(ActuatorId id) => new(id.Value);
}
