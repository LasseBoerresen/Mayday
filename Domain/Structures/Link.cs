using Domain.Geometry;

namespace Domain.Structures;

public class Link(Pose origin) : Component(origin)
{
    // public void SetParent(Connection parent) => Parent = parent;

    public static Link Base => new(Pose.Zero);
};
