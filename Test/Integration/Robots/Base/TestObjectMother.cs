using Robots;
using RobotDomain.Time;

namespace Test.Integration.Robots.Base;

/// <inheritdoc/>
internal class TestObjectMother
{
    internal static MaydayRobotFactory MaydayRobotFactory
        // Builds the real hardware factory, which physical tests use, so keep the production fail-fast behavior.
        => new(MaydayDomain.Base.TestObjectMother.LegPostureByPositionMap, new FailFastFatalErrorHandler());
}
