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

        try
        {
            var may = MaydayRobot.CreateWithBabyLegsBehaviorController();
            may.Start();
            builder.Services.AddSingleton(may);
        }
        catch (FailedToOpenPortException e)
        {
            Console.WriteLine(e);
        } 
        
        return builder.Build();
    }
}
