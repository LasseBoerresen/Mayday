using LanguageExt;
using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public class Attachment : Connection
{
    readonly Transform _transform;

    protected override Transform Transform => _transform;

    Attachment(ComponentId id, Link parent, Link child, Transform transform) 
        : base(id, parent, child)
    {
        _transform = transform;
    }

    public static Attachment NewBetween(Link parent, Link child, Transform transform) => 
        new(ComponentId.New, parent, child, transform);
}