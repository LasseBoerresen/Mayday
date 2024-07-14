using LanguageExt;
using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public class Attachment : Connection
{
    readonly Pose _pose;

    public override Pose Pose => _pose;

    Attachment(ComponentId id, Link parent, Link child, Pose pose) 
        : base(id, parent, child)
    {
        _pose = pose;
    }

    public static Attachment NewBetween(Link parent, Link child, Pose pose) => 
        new(ComponentId.New, parent, child, pose);
}