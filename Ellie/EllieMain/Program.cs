using Dynamixel;
using EllieMain;
using EllieMain.Behaviors;
using EllieMain.MotionPlanning;

var ellie = CreateEllieFactory().CreateDefault();

await ellie.Start();
return;

EllieFactory CreateEllieFactory()
{
    CancellationTokenSource cts = new();

    DynamixelWheelController wheelController = new();
    ArticulatedSteeringEllieMotionPlanner motionPlanner = new(wheelController);
    TerminalMovementBehaviorController behaviorController = new(motionPlanner, cts.Token);

    return new EllieFactory(behaviorController, cts);
}