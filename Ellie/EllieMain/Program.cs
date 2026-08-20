using Dynamixel;
using EllieMain;
using EllieMain.Base;
using EllieMain.Behaviors;
using EllieMain.MotionPlanning;
using Generic.System;
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
        .Map(ellie => ellie.Start());
}

Eff<Ellie> CreateEllieEff<RT>()
{
    var communicationBusEff = NativeSerialPortCommunicationBus.CreateInitialized();
    
    return communicationBusEff.Map(CreateEllie);
} 

Ellie CreateEllie(CommunicationBus communicationBus)
{
    CancellationTokenSource cts = new();

    // TODO Replace with Language.Ext.Runtime.Console to be more safe
    SystemTerminal terminal = new();
    // TODO replace with Language.Ext.Runtime.TimeProvider to be more safe
    var timeProvider = TimeProvider.System;
    
    Dynamixel.Driver driver = new(communicationBus);
    PeriodicallyBatchedWheelDriver wheelDriver = new(driver);
    ArticulatedSteeringEllieMotionPlanner motionPlanner = new(wheelDriver);
    TerminalMovementBehaviorController behaviorController = new(
        motionPlanner, 
        terminal,
        timeProvider,
        cts.Token);

    EllieFactory ellieFactory = new(behaviorController, cts);
    
    return ellieFactory.CreateDefault();
}