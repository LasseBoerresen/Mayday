using mayday.Structures;

namespace mayday;

public class Joint(Link? Parent, Link? Child) : Connection(Parent, Child)
{
    public JointState State { get; }
}
