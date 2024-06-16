using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public class Attachment(Pose origin, Link parent, Link child)
    : Connection(origin, parent, child);
