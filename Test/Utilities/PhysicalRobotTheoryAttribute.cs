using Xunit;
using Xunit.Sdk;

namespace Test.Utilities;

/// <summary>
/// The theory form of <see cref="PhysicalRobotFactAttribute"/>: skips unless the robot is clear to move, and carries the
/// trait <c>Tier=Physical</c>, which the Stop hook's gate excludes; the pre-push gate runs it on the robot (ADR 0011).
/// </summary>
[TraitDiscoverer("Test.Utilities.PhysicalRobotTraitDiscoverer", "Test")]
[AttributeUsage(AttributeTargets.Method)]
public sealed class PhysicalRobotTheoryAttribute : TheoryAttribute, ITraitAttribute
{
    public PhysicalRobotTheoryAttribute()
    {
        if (!TestConfiguration.IsRobotClearToMove())
            Skip = TestConfiguration.MaydayRobotIsNotClearToMoveReason;
    }
}
