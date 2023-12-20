using mayday.Geometry;
using mayday.Structures;

namespace mayday;

public class Attachment(Pose origin, Link? parent, Link? child)
    : Connection(origin, parent, child);
