using mayday.Geometry;
using Mayday.Geometry;
using Mayday.Structures;

namespace Mayday.Structures;

public class Link(Pose origin) : Component(origin)
{
    // public void SetParent(Connection parent) => Parent = parent;

    public static Link Base => new(Pose.Zero);
};
