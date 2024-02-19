using Mayday.Geometry;

namespace Mayday.Structures;

public class Attachment(Pose origin, Link parent, Link child)
    : Connection(origin, parent, child);
