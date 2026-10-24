namespace Test.Utilities;
using Xunit;
using Xunit.Sdk;

/// <summary>
/// A fact that drives the physical robot. It skips unless <see cref="TestConfiguration.IsRobotConnected()"/>, and
/// carries the trait <c>Tier=Physical</c> so a run can exclude it by filter (<c>Tier!=Physical</c>).
/// </summary>
[TraitDiscoverer("Test.Utilities.PhysicalTraitDiscoverer", "Test")]
public sealed class PhysicalRobotFactAttribute : FactAttribute, ITraitAttribute
{
    public PhysicalRobotFactAttribute()
    {
        if (!TestConfiguration.IsRobotConnected())
            Skip = TestConfiguration.MaydayRobotIsNotConnectedReason;
    }
}
