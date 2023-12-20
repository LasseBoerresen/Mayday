using mayday.Geometry;
using mayday.Structures;

namespace Test.TestStructures;

public class ZeroStateFakeJoint(Link parent, Link child)
    : Joint(Pose.Zero, new ZeroStateFakeMotor(), parent, child);
