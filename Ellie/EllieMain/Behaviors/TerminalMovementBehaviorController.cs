using EllieMain.MotionPlanning;
using Generic.System;
using RobotDomain.Behavior;

namespace EllieMain.Behaviors;

public class TerminalMovementBehaviorController(
        EllieMotionPlanner motionPlanner,
        Terminal terminal,
        CancellationToken ct) 
    : TerminalBehaviorController<MovementCommand>(terminal, ct)
{
    protected override void WakeUpBehavior()
    {
        motionPlanner.Start();
    }

    protected override void SleepBehavior()
    {
        throw new NotImplementedException();
    }

    protected override void ExecuteCommand(MovementCommand command)
    {
        throw new NotImplementedException();
    }

    protected override void PrintUpdate()
    {
        throw new NotImplementedException();
    }
}

public enum MovementCommand
{
}