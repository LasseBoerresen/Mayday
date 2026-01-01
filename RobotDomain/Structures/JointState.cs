using RobotDomain.Physics;
using RobotDomain.Time;
using UnitsNet;

namespace RobotDomain.Structures;

public record JointState(
    Angle Angle,
    RotationalSpeed RotationalSpeed,
    LoadRatio Torque,
    Temperature Temperature,
    Timed<Angle> AngleGoal)
{
    public static JointState Zero => new(
        Angle.Zero,
        RotationalSpeed.Zero,
        LoadRatio.Zero,
        Temperature.Zero,
        Timed<Angle>.Passed(Angle.Zero));
    
    public Angle InterpolateGoalAngleOneTimeStep(Func<Timed<Angle>, double> interpolatedStepFactorFunc)
    {
        var remainingAngle = AngleGoal.Target - Angle;
        
        return AngleGoal.Target + remainingAngle * interpolatedStepFactorFunc(AngleGoal);
    }
};
