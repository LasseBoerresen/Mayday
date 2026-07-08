using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;

namespace MaydayDomain;

public interface MaydayStructure
{
    MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName);

    Xyz GetPositionOf(LinkName linkName, MaydayLegId legId);

    MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName);

    MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName);

    void SetPosture(Timed<MaydayStructurePosture> postureTimed);

    MaydayStructureSet<MaydayLegPosture> GetPostures();

    MaydayLegPosture GetPostureOf(MaydayLegId legId);

    void SetPostureForAllLegs(Timed<MaydayLegPosture> postureTimed);

    /// <summary>
    /// To lean the thorax, move all tips opposite direction. Rotational lean
    /// results in some translation of the tip around the thorax origo 
    /// </summary>
    void MoveThoraxTo(Timed<Transform> leanTimed);

    void MoveTipsBy(Timed<Xyz> offsetXyzTimed);

    void MoveTipsTo(Timed<MaydayStructureSet<Xyz>> tipPositionsTimed);

    Transform GetCurrentLean();
}
