using UnitsNet;

namespace mayday.Structures;

public record JointState(
    Angle Angle,
    RotationalSpeed RotationalSpeed,
    Torque Torque,
    Temperature Temperature,
    Angle AngleGoal);
