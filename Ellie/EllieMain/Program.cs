using Dynamixel;
using EllieMain;
using EllieMain.Behaviors;
using EllieMain.MotionPlanning;
using RobotDomain.Motion;

var ellie = CreateEllieFactory().CreateDefault();

await ellie.Start();
return;

EllieFactory CreateEllieFactory()
{
    CancellationTokenSource cts = new();

    PeriodicallyBatchedWheelDriver wheelDriver = new();
    ArticulatedSteeringEllieMotionPlanner motionPlanner = new(wheelDriver);
    TerminalMovementBehaviorController behaviorController = new(motionPlanner, cts.Token);

    return new EllieFactory(behaviorController, cts);
}