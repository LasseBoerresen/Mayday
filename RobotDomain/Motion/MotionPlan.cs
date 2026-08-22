using RobotDomain.Time;

namespace RobotDomain.Motion;

/// <summary>
/// A plan of robot state through time
/// </summary>
/// <typeparam name="TState">
/// State a robot structure can move towards, e.g. a position, continued motion,
/// or other
/// </typeparam>
public interface MotionPlan<TState>
{
    Timed<TState> At(DateTime time);
};
