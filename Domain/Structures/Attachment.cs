using Domain.Geometry;

namespace Domain.Structures;

public class Attachment(Pose origin, Link parent, Link child)
    : Connection(origin, parent, child);
