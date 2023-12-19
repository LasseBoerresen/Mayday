using mayday.Geometry;

namespace mayday.Structures;

public class Link(Pose origin, Connection? parent = null, Connection? child = null)
    : Component(origin, parent, child)
{
    public static Link Base => new(Pose.Zero);
};
