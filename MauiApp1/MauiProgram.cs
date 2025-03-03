using Dynamixel;
using ManualBehavior;
using Microsoft.Extensions.Logging;

namespace MauiApp1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

        #if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
        #endif

        // todo: show the mayday component, no matter if the robot is connected or not. 
        //       with an option to press a button to reconnect. 
        try
        {
            var may = MaydayRobot.CreateWithBabyLegsBehaviorController();
            may.Start();
            builder.Services.AddSingleton(may);
        }
        catch (FailedToOpenPortError e)
        {
            Console.WriteLine(e);
        } 
        
        return builder.Build();
    }
}
