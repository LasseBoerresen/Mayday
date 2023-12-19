using mayday.Geometry;
using mayday.Structures;

namespace Test.TestStructures;

public class ZeroStateFakeJoint() : Joint(Pose.Zero, new ZeroStateFakeMotor());
