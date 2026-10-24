using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.Utilities;

/// <summary>
/// Marks a test that drives the real robot and asks the watching human, through <see cref="HumanVerifier"/>,
/// whether the robot did what the test expects. Use it for behavior no sensor can report yet.
/// </summary>
/// <remarks>
/// Skips unless both <see cref="TestConfiguration.IsRobotConnected"/> and <see cref="TestConfiguration.IsHumanPresent"/>
/// hold. Carries the trait <c>Tier=Manual</c>, so <c>dotnet test --filter "Tier=Manual"</c> runs only these tests.
/// </remarks>
[TraitDiscoverer("Test.Utilities.ManualRobotTraitDiscoverer", "Test")]
[AttributeUsage(AttributeTargets.Method)]
public sealed class ManualRobotFactAttribute : FactAttribute, ITraitAttribute
{
    public ManualRobotFactAttribute()
    {
        if (!TestConfiguration.IsRobotConnected())
            Skip = TestConfiguration.MaydayRobotIsNotConnectedReason;
        else if (!TestConfiguration.IsHumanPresent())
            Skip = TestConfiguration.MaydayHumanIsNotPresentReason;
    }
}

/// <summary>Turns <see cref="ManualRobotFactAttribute"/> into the filterable trait xUnit reports.</summary>
public sealed class ManualRobotTraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        yield return new("Tier", "Manual");
    }
}
