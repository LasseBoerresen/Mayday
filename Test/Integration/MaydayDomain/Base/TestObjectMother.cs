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

    /// <summary>
    /// A new hardware-free structure of echo joints that looks postures up in <see cref="DefaultLegPostureByPositionMap"/>.
    /// </summary>
    internal static DefaultMaydayStructure CreateEchoStructureWithDefaultMap() =>
        new MaydayStructureFactory(new MaydayLegFactory(new EchoJointFactory(), DefaultLegPostureByPositionMap))
            .CreateDefault();

    /// <summary>
    /// A new hardware-free leg of echo joints that looks postures up in <see cref="DefaultLegPostureByPositionMap"/>.
    /// </summary>
    internal static MaydayLeg CreateEchoLegWithDefaultMap(MaydayLegId legId) =>
        new MaydayLegFactory(new EchoJointFactory(), DefaultLegPostureByPositionMap).CreateLeg(legId);

    // Loaded once from the copy MaydayDataAccess places in the output directory.
    static readonly Lazy<LegPostureByPositionMap> DefaultMap = new(() =>
        LegPostureByPositionMapFileRepo.ForDefaultMapIn(new DirectoryInfo(AppContext.BaseDirectory)).Load());
}
