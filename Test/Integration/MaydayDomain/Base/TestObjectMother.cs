using MaydayDomain;

namespace Test.Integration.MaydayDomain.Base;

/// <inheritdoc/>
public class TestObjectMother : Unit.Base.TestObjectMother
{
    internal static LegPostureByPositionMap LegPostureByPositionMap 
        => LegPostureByPositionMap.CreateEmpty();
}
