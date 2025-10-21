using LanguageExt;
using LanguageExt.Common;
using ManualBehavior;

StartupMode startupMode = Enum.Parse<StartupMode>(Environment.GetEnvironmentVariable("mayday_startup_mode") ?? "");

if (startupMode == StartupMode.Run)
    Run(MaydayRobot.CreateWithTerminalPostureBehaviorController());
else if (startupMode == StartupMode.Train)
    Run(MaydayRobot.CreateWithBabyLegsBehaviorController());

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