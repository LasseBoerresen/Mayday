using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Sys.Live;
using Main;
using ManualBehavior;
using MaydayDomain.MotionPlanning;
using Robots;
using Robots.Base;


Main().Run(Runtime.New())
    .ThrowIfFail();
        
static Eff<Runtime, Unit> Main()
{
    var timeProvider = TimeProvider.System;

    return CreateRobot<Runtime>(timeProvider)
        .Map(may => may.Start());
}

static Eff<MaydayRobot> CreateRobot<RT>(TimeProvider timeProvider)
{
    StartupMode startupMode = Enum.Parse<StartupMode>(Environment.GetEnvironmentVariable("mayday_startup_mode") ?? "");
    
    return startupMode switch
    {
        StartupMode.Run => MaydayRobotFactory.CreateWithSwayBehavior(timeProvider),
        StartupMode.Train => MaydayRobotFactory.CreateWithBabyLegsBehaviorController(timeProvider),
        _ => Error.New("Invalid startup mode: " + startupMode)
    };
}

Unit PrintErrorToConsole(Error error)
{
    Console.WriteLine(error.ToString());
    
    return Unit.Default;
}