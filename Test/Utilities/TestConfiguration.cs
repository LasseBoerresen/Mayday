using Microsoft.Extensions.Configuration;

namespace Test.Utilities;

public class TestConfiguration
{
    const string MaydayRobotIsConnectedConfigKey = "MAYDAY_ROBOT_IS_CONNECTED";

    public const string MaydayRobotIsNotConnectedReason =
        $"Robot is not connected, set config key '{MaydayRobotIsConnectedConfigKey}' "
        + $"to 'true' to run this test.";
    
    const string MaydayHumanIsPresentConfigKey = "MAYDAY_HUMAN_IS_PRESENT";

    public const string MaydayHumanIsNotPresentReason =
        $"No human is present to answer, set config key '{MaydayHumanIsPresentConfigKey}' "
        + $"to 'True' to run this manual test.";

    public static bool IsRobotConnected()
    {
        // null config value is counted as not connected.
        var configValue = Create()[MaydayRobotIsConnectedConfigKey];

        return configValue == "True";
    }

    /// <summary>
    /// Whether a person is at the robot to answer manual tests. Kept apart from <see cref="IsRobotConnected"/>, so an
    /// unattended physical run is never left waiting for an answer.
    /// </summary>
    public static bool IsHumanPresent()
    {
        // null config value is counted as not present.
        var configValue = Create()[MaydayHumanIsPresentConfigKey];

        return configValue == "True";
    }
    
    public static IConfiguration Create()
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables()
            // Optional: a machine without the secrets file, such as a CI runner, has no robot, which
            // IsRobotConnected treats as not connected. Physical tests still need an explicit 'True' to run.
            .AddUserSecrets<TestConfiguration>(optional: true)
            .Build();
    }
}
