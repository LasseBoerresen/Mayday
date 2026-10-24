using Xunit;
using Xunit.Sdk;

namespace Test.Utilities;

/// <summary>
/// A theory that drives the physical robot. It skips unless <see cref="TestConfiguration.IsRobotConnected()"/>,
/// and carries the trait <c>Tier=Physical</c> so a run can exclude it by filter (<c>Tier!=Physical</c>).
/// </summary>
[TraitDiscoverer("Test.Utilities.PhysicalTraitDiscoverer", "Test")]
public sealed class PhysicalRobotTheoryAttribute : TheoryAttribute, ITraitAttribute
{
    public PhysicalRobotTheoryAttribute()
    {
        if (!TestConfiguration.IsRobotConnected())
            Skip = TestConfiguration.MaydayRobotIsNotConnectedReason;
    }
}

/// <summary>Gives <see cref="PhysicalRobotFactAttribute"/> and <see cref="PhysicalRobotTheoryAttribute"/> tests the filterable trait <c>Tier=Physical</c>.</summary>
public sealed class PhysicalTraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(Xunit.Abstractions.IAttributeInfo traitAttribute)
    {
        yield return new("Tier", "Physical");
    }
}
