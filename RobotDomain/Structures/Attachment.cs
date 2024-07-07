using LanguageExt;
using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public class Attachment : Connection

{
    Attachment(ComponentId id, Link parent, Link child) 
        : base(id, parent, child)
    {
    }

    public static Attachment NewBetween(Link parent, Link child) => 
        new(ComponentId.New, parent, child);
}