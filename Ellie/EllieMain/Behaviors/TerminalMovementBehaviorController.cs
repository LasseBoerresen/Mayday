using EllieMain.MotionPlanning;
using Generic.System;
using RobotDomain.Behavior;
using RobotDomain.Time;
using UnitsNet;

namespace EllieMain.Behaviors;

public class TerminalMovementBehaviorController(
        EllieMotionPlanner motionPlanner,
        Terminal terminal,
        TimeProvider timeProvider,
        CancellationToken ct) 
    : TerminalBehaviorController<MovementCommand>(terminal, ct)
{
    protected override void WakeUpBehavior()
    {
        motionPlanner.Start(ct);
    }

    protected override void SleepBehavior()
    {
        throw new NotImplementedException();
    }

    protected override void ExecuteCommand(MovementCommand command)
    {
        switch (command)
        {
            case MovementCommand.AccelerateForward:
                motionPlanner.AccelerateBy(Schedule(Speed.FromMetersPerSecond(0.01)));
                break;
            case MovementCommand.AccelerateReverse:
                motionPlanner.AccelerateBy(Schedule(Speed.FromMetersPerSecond(-0.01)));
                break;
            case MovementCommand.Brake:
                break;
            case MovementCommand.TurnLeft:
                break;
            case MovementCommand.TurnRight:
                break;
            case MovementCommand.StraightenUp:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, null);
        }
    }
    
    Timed<T> Schedule<T>(T target) => 
        timeProvider.ScheduleIn(target, TimeSpan.FromSeconds(1));

    protected override void PrintUpdate()
    {
        var updateText = motionPlanner.Goal
            .Some(g => 
                $"Goal: \n"
                + $"Arrival time: {g.ArrivalTime}\n"
                + $"Target:       {g.Target}")
            .None(() => "No goal");
            
        terminal.WriteLine(updateText);
        terminal.WriteLine("");
    }
}