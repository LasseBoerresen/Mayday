using mayday.Geometry;

namespace mayday.Structures;

public abstract class Component(Pose origin, Component? Parent = null, Component? Child = null)
{
    public Pose Origin => origin;
}
