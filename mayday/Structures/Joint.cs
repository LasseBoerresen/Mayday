using mayday.Geometry;

namespace mayday.Structures;

public class Joint(Pose origin, Motor motor, Link? parent=null, Link? child=null)
    : Connection(origin, parent, child)
{
    public JointState State => motor.State;
}
