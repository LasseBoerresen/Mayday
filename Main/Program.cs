using LanguageExt;
using Main;

StartupMode startupMode = Enum.Parse<StartupMode>(Environment.GetEnvironmentVariable("mayday_startup_mode") ?? "");

if (startupMode == StartupMode.Run)
{
    MaydayRobot
        .CreateWithTerminalPostureBehaviorController()
        .Start();
}
else if (startupMode == StartupMode.Train)
{
    MaydayRobot
        .CreateWithBabyLegsBehaviorController()
        .Start();
}
