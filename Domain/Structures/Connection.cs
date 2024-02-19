using Mayday.Geometry;

namespace Mayday.Structures;

public class Connection : Component
{
    public Connection(Pose origin, Link parent, Link child)
        : base(origin, parent, child)
    {
        parent.Child = this;
        child.Parent = this;
    }
}
