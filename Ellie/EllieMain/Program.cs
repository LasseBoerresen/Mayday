using Dynamixel;
using EllieMain;
using EllieMain.Base;
using EllieMain.Behaviors;
using EllieMain.MotionPlanning;
using LanguageExt;
using LanguageExt.Sys.Live;
using RobotDomain.Motion;

Main()
    .Run(Runtime.New())
    .ThrowIfFail();

return;

Eff<Runtime, Unit> Main()
{
    // var timeProvider = TimeProvider.System;

    return CreateEllieEff<Runtime>()
        .Map(may => may.Start());
}

Eff<Ellie> CreateEllieEff<RT>()
{
    var communicationBusEff = NativeSerialPortCommunicationBus.CreateInitialized();
    
    return communicationBusEff.Map(CreateEllie);
} 

Ellie CreateEllie(CommunicationBus communicationBus)
{
    CancellationTokenSource cts = new();
    
    Dynamixel.Driver driver = new(communicationBus);
    PeriodicallyBatchedWheelDriver wheelDriver = new(driver);
    ArticulatedSteeringEllieMotionPlanner motionPlanner = new(wheelDriver);
    TerminalMovementBehaviorController behaviorController = new(motionPlanner, cts.Token);

    EllieFactory ellieFactory = new(behaviorController, cts);
    
    return ellieFactory.CreateDefault();
}