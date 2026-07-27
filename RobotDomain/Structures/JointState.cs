using RobotDomain.Physics;
using RobotDomain.Time;
using UnitsNet;

namespace RobotDomain.Structures;

public record JointState(
    Angle Angle,
    RotationalSpeed RotationalSpeed,
    LoadRatio Torque,
    Temperature Temperature,
    Timed<Angle> AngleGoal,
    Timed<Angle> AngleGoalPrevious)
{
    public static JointState Zero
    {
        get => new(
            Angle.Zero,
            RotationalSpeed.Zero,
            LoadRatio.Zero,
            Temperature.Zero,
            Timed<Angle>.Passed(Angle.Zero),
            Timed<Angle>.Passed(Angle.Zero));
    }

    public Angle InterpolateGoalAngleOneTimeStep(Func<Timed<Angle>, double> timedStepFactorFunc)
    {
        var stepFactor = timedStepFactorFunc(AngleGoal);
        var angleDiff = AngleGoal.Target - AngleGoalPrevious.Target;

        var angleStep = angleDiff * stepFactor;

        return AngleGoalPrevious.Target + angleStep;
    }
};
