using UnitsNet;

namespace Domain.Structures;

public record JointState(
    Angle Angle,
    RotationalSpeed RotationalSpeed,
    Torque Torque,
    UnitsNet.Temperature Temperature,
    Angle? AngleGoal)
{
    public static JointState Zero => new(
        Angle.Zero,
        RotationalSpeed.Zero,
        Torque.Zero,
        UnitsNet.Temperature.Zero,
        Angle.Zero);
};
