using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Sys.Live;
using Main;
using MaydayDataAccess;
using Robots;
using Robots.Base;

Main().Run(Runtime.New())
    .ThrowIfFail();
        
static Eff<Runtime, Unit> Main()
{
    var timeProvider = TimeProvider.System;

    return CreateRobotEff<Runtime>(timeProvider)
        .Map(may => may.Start());
}

static Eff<MaydayRobot> CreateRobotEff<RT>(
    TimeProvider timeProvider)
{
    var maydayRobotEff = 
        CreateMaydayRobotFactory<RT>().Bind(maydayFactory =>
        GetStartupMode<RT>().Bind(startupMode => 
            CreateGivenMode(startupMode, maydayFactory)));

    return maydayRobotEff;

    Eff<MaydayRobot> CreateGivenMode(StartupMode startupMode, MaydayRobotFactory maydayFactory)
    {
        return startupMode switch
        {
            StartupMode.Run => maydayFactory.CreateWithSwayBehavior(timeProvider),
            StartupMode.Train => maydayFactory.CreateWithBabyLegsBehaviorController(timeProvider),
            _ => Error.New("Invalid startup mode: " + startupMode)
        };
    } 
}

static Eff<StartupMode> GetStartupMode<RT>()
{
    if (Environment.GetEnvironmentVariable("mayday_startup_mode") is not { } startupModeString)
        return Error.New("No startup mode specified in environment variable: mayday_startup_mode");
    
    if(!Enum.TryParse<StartupMode>(startupModeString, out var startupMode))
        return Error.New("Invalid startup mode: " + startupModeString);
    
    return Eff<StartupMode>.Pure(startupMode); 
}

static Eff<MaydayRobotFactory> CreateMaydayRobotFactory<RT>()
{
    // TODO handle that reading file can fail and will produce Error or EFF.Pure
    FileInfo legPostureByPositionMapFileInfo = new("asdfasdfMaydayLegPostureMap.json");
    LegPostureByPositionMapFileRepo legPostureByPositionMapFileRepo = new(legPostureByPositionMapFileInfo);
    var legPostureByPositionMap = legPostureByPositionMapFileRepo.Load();
    
    return Eff<MaydayRobotFactory>.Pure(new(legPostureByPositionMap));
}

Unit PrintErrorToConsole(Error error)
{
    Console.WriteLine(error.ToString());
    
    return Unit.Default;
}