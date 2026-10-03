using MaydayDomain;
using RobotDomain.Geometry;

namespace Test.Integration.MaydayDomain.Base;

/// <inheritdoc/>
public class TestObjectMother : Unit.Base.TestObjectMother
{
    /// <summary>
    /// A map with one mapping, because <see cref="LegPostureByPositionMapDictImpl"/> rejects an empty map.
    /// Positions without a mapping fall back to the current posture, so tests that do not look up postures
    /// are unaffected.
    /// </summary>
    internal static LegPostureByPositionMap LegPostureByPositionMap
        => new LegPostureByPositionMapDictImpl(
            new Dictionary<Xyz, List<MaydayLegPosture>> { [Xyz.Zero] = [MaydayLegPosture.Neutral] });
}
