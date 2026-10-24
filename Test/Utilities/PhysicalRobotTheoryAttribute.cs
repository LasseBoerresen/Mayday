using Xunit;
using Xunit.Sdk;

namespace Test.Utilities;

/// <summary>
/// The theory form of <see cref="PhysicalRobotFactAttribute"/>: skips unless the robot is connected, and carries the
/// trait <c>Tier=Physical</c>, which the gate excludes.
/// </summary>
[TraitDiscoverer("Test.Utilities.PhysicalRobotTraitDiscoverer", "Test")]
[AttributeUsage(AttributeTargets.Method)]
public sealed class PhysicalRobotTheoryAttribute : TheoryAttribute, ITraitAttribute
{
    public PhysicalRobotTheoryAttribute()
    {
        if (!TestConfiguration.IsRobotConnected())
            Skip = TestConfiguration.MaydayRobotIsNotConnectedReason;
    }
}
