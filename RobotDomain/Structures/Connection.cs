using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public class Connection : Component
{
    public Connection(Pose origin, Link parent, Link child)
        : base(origin, parent, child)
    {
        parent.Child = this;
        child.Parent = this;
    }
}
