using UnitsNet;

namespace mayday.Structures;

public record JointState(
    Angle Angle,
    RotationalSpeed RotationalSpeed,
    Torque Torque,
    Temperature Temperature,
    Angle? AngleGoal = null)
{
    public static JointState Zero => new JointState(
        Angle.Zero,
        RotationalSpeed.Zero,
        Torque.Zero,
        Temperature.Zero,
        Angle.Zero);
};
