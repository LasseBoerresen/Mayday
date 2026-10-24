using Microsoft.Extensions.Configuration;
using Test.Utilities;
using Xunit;

namespace Test.Unit.Utilities;

public class TestConfigurationTests
{
    const string RobotIsConnectedKey = "MAYDAY_ROBOT_IS_CONNECTED";

    [Theory]
    [InlineData("True", "False", false)]
    [InlineData("False", "True", true)]
    [InlineData("True", null, true)]
    public void GivenUserSecretsAndEnvironment_WhenIsRobotConnected_ThenEnvironmentDecides(
        string? userSecretsValue, string? environmentValue, bool expected)
    {
        // Given
        var configuration = TestConfiguration.Create(
            addUserSecrets: b => b.AddInMemoryCollection(Values(userSecretsValue)),
            addEnvironmentVariables: b => b.AddInMemoryCollection(Values(environmentValue)));

        // When
        var actual = TestConfiguration.IsRobotConnected(configuration);

        // Then
        Assert.Equal(expected, actual);
    }

    static Dictionary<string, string?> Values(string? robotIsConnected) =>
        robotIsConnected is null ? [] : new() { [RobotIsConnectedKey] = robotIsConnected };
}
