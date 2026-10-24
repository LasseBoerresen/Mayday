using Microsoft.Extensions.Configuration;

namespace Test.Utilities;

public class TestConfiguration
{
    const string MaydayRobotIsConnectedConfigKey = "MAYDAY_ROBOT_IS_CONNECTED";

    public const string MaydayRobotIsNotConnectedReason =
        $"Robot is not connected, set config key '{MaydayRobotIsConnectedConfigKey}' "
        + $"to 'true' to run this test.";

    /// <summary>
    /// Whether a physical robot is attached, from <c>MAYDAY_ROBOT_IS_CONNECTED</c>. An environment variable overrides
    /// the user secrets, so a caller that sets it to <c>False</c>, such as the gate, keeps physical tests skipped.
    /// </summary>
    public static bool IsRobotConnected() => IsRobotConnected(Create());

    internal static bool IsRobotConnected(IConfiguration configuration)
    {
        // null config value is counted as not connected.
        return configuration[MaydayRobotIsConnectedConfigKey] == "True";
    }

    public static IConfiguration Create() => Create(
        // Optional: a machine without the secrets file, such as a CI runner, has no robot, which
        // IsRobotConnected treats as not connected. Physical tests still need an explicit 'True' to run.
        addUserSecrets: b => b.AddUserSecrets<TestConfiguration>(optional: true),
        addEnvironmentVariables: b => b.AddEnvironmentVariables());

    internal static IConfiguration Create(
        Action<IConfigurationBuilder> addUserSecrets,
        Action<IConfigurationBuilder> addEnvironmentVariables)
    {
        var builder = new ConfigurationBuilder();
        addUserSecrets(builder);
        // The source added last wins, so the environment is added after the secrets to override them.
        addEnvironmentVariables(builder);
        return builder.Build();
    }
}
