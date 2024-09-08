using RobotDomain.Physics;
using UnitsNet;

namespace RobotDomain.Structures;

public record JointState(
    Angle Angle,
    RotationalSpeed RotationalSpeed,
    LoadRatio Torque,
    UnitsNet.Temperature Temperature,
    Angle? AngleGoal)
{
    public static JointState Zero => new(
        Angle.Zero,
        RotationalSpeed.Zero,
        LoadRatio.Zero,
        UnitsNet.Temperature.Zero,
        Angle.Zero);
};
