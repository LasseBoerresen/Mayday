using MaydayDataAccess;
using MaydayDomain;

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
        => LegPostureByPositionMapDictImpl.CreateNeutral();

    /// <summary>
    /// The map the robot uses. Use it when a test moves tips, since the neutral map cannot find a posture for any
    /// position but the origin.
    /// </summary>
    internal static LegPostureByPositionMap DefaultLegPostureByPositionMap => DefaultMap.Value;

    // Loaded once from the copy MaydayDataAccess places in the output directory.
    static readonly Lazy<LegPostureByPositionMap> DefaultMap = new(() =>
        LegPostureByPositionMapFileRepo.ForDefaultMapIn(new DirectoryInfo(AppContext.BaseDirectory)).Load());
}
