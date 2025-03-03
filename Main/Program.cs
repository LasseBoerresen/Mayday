using LanguageExt;
using LanguageExt.Common;
using ManualBehavior;

StartupMode startupMode = Enum.Parse<StartupMode>(Environment.GetEnvironmentVariable("mayday_startup_mode") ?? "");

if (startupMode == StartupMode.Run)
{
    MaydayRobot.CreateWithTerminalPostureBehaviorController()
        .Map(robot => robot.Start())
        .Run()
        .BindFail(error => PrintErrorToConsole(error));
}
else if (startupMode == StartupMode.Train)
{
    MaydayRobot
        .CreateWithBabyLegsBehaviorController()
        .Map(robot => robot.Start())
        .Run()
        .BindFail(error => PrintErrorToConsole(error));
}

Unit PrintErrorToConsole(Error error)
{
    Console.WriteLine(error.ToString());
    
    return Unit.Default;
}

