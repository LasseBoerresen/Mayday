using mayday.Structures;

namespace Test.TestStructures;

public class ZeroStateFakeMotor : Motor
{
    public JointState State => JointState.Zero;
}
