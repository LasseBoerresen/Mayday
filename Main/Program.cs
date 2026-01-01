using LanguageExt;
using LanguageExt.Common;
using ManualBehavior;

StartupMode startupMode = Enum.Parse<StartupMode>(Environment.GetEnvironmentVariable("mayday_startup_mode") ?? "");

var timeProvider = TimeProvider.System;

if (startupMode == StartupMode.Run)
    Run(MaydayRobot.CreateWithTerminalPostureBehaviorController(timeProvider));
else if (startupMode == StartupMode.Train)
    Run(MaydayRobot.CreateWithBabyLegsBehaviorController(timeProvider));

void Run(Eff<MaydayRobot> maydayRobotEffect)
{
    maydayRobotEffect
        .Map(robot => robot.Start())
        .Run()
        .BindFail(error => PrintErrorToConsole(error));
}

Unit PrintErrorToConsole(Error error)
{
    Console.WriteLine(error.ToString());
    
    return Unit.Default;
}