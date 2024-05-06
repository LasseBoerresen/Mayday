using Domain.Geometry;

namespace Domain.Structures;

public class Joint(Pose origin, Motor motor, Link parent, Link child)
    : Connection(origin, parent, child)
{
    public JointState State => motor.State;
}
