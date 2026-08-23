using RobotDomain.Structures;

namespace EllieMain.Structures;

public record State(
    WheelState LF,
    WheelState RF,
    WheelState LB,
    WheelState RB,
    JointState Articulation);