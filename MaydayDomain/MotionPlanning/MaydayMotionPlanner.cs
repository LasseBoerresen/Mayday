using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public interface MaydayMotionPlanner
{
    MaydayStructureSet<MaydayLegPosture> GetPosture();
    void SetPosture(MaydayStructurePosture posture);
    void SetPosture(MaydayLegPosture posture);
    MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName);
    MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName);
    MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName);
}