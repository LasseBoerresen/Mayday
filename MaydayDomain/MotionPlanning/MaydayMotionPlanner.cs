using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public interface MaydayMotionPlanner
{
    MaydayStructureSet<MaydayLegPosture> GetPostures();
    MaydayLegPosture GetPostureOf(MaydayLegId legId);
    void MoveTipPositions(MaydayStructureSet<Xyz> tipDeltas);
    void SetPosture(MaydayStructurePosture posture);
    void SetPosture(MaydayLegPosture posture);
    MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName);
    Xyz GetPositionOf(LinkName linkName, MaydayLegId legId);
    MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName);
    MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName);
}