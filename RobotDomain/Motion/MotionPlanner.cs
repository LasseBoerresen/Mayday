using LanguageExt;
using RobotDomain.Time;

namespace RobotDomain.Motion;

// TODO In principle motion planners should be able to take 3 types of commands. 
//   Position, speed and posture commands. Different applications require each one. 
//   When I move my arm, I can either chose to move it to an absolute position,
//   move with a constant speed or set my joints to a certain angle.
//   So, instead of just a TMotion, there might be 3 different motions. Or
//   TMotion can contain a union of either target type. 

/// <summary>
/// When started, continuously tracks a goal state of a Structure by
/// planning and executing a trajectory, which may require nonlinear actuator
/// subgoals.
/// </summary>
public interface MotionPlanner<TState>
{
    void Start(CancellationToken ct);

    Option<Timed<TState>> Goal { get; set; }
    Option<MotionPlan<TState>> Plan { get; }
};