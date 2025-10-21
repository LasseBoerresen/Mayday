using Microsoft.Extensions.Configuration;

namespace Test.Utilities;

public class TestConfiguration
{
    const string MaydayRobotIsConnectedConfigKey = "MAYDAY_ROBOT_IS_CONNECTED";

    public const string MaydayRobotIsNotConnectedReason =
        $"Robot is not connected, set config key '{MaydayRobotIsConnectedConfigKey}' "
        + $"to 'true' to run this test.";
    
    public static bool IsRobotConnected()
    {
        // null config value is counted as not connected. 
        var configValue = Create()[MaydayRobotIsConnectedConfigKey];
        
        return configValue == "True";
    }
    
    public static IConfiguration Create()
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<TestConfiguration>(optional: false)
            .Build();
    }
}
