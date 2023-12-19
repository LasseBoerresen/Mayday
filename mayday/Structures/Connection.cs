using mayday.Geometry;
using mayday.Structures;

namespace mayday;

public class Connection(Pose origin, Link? parent, Link? child)
    : Component(origin, parent, child);
