using mayday.Geometry;
using mayday.Structures;

namespace mayday;

public class Attachment(Pose origin, Link? parent = null, Link? child = null)
    : Connection(origin, parent, child);
