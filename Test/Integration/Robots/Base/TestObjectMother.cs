using Robots;

namespace Test.Integration.Robots.Base;

/// <inheritdoc/>
internal class TestObjectMother
{
    internal static MaydayRobotFactory MaydayRobotFactory
        => new(MaydayDomain.Base.TestObjectMother.LegPostureByPositionMap);
}
