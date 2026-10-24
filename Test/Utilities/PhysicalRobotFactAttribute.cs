namespace Test.Utilities;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

/// <summary>
/// Marks a test that drives the real robot. Skips unless <see cref="TestConfiguration.IsRobotClearToMove"/> holds, and
/// carries the trait <c>Tier=Physical</c>, which the gate excludes.
/// </summary>
[TraitDiscoverer("Test.Utilities.PhysicalRobotTraitDiscoverer", "Test")]
[AttributeUsage(AttributeTargets.Method)]
public sealed class PhysicalRobotFactAttribute : FactAttribute, ITraitAttribute
{
    public PhysicalRobotFactAttribute()
    {
        if (!TestConfiguration.IsRobotClearToMove())
            Skip = TestConfiguration.MaydayRobotIsNotClearToMoveReason;
    }
}

/// <summary>Turns the physical robot test attributes into the filterable trait xUnit reports.</summary>
public sealed class PhysicalRobotTraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        yield return new("Tier", "Physical");
    }
}
