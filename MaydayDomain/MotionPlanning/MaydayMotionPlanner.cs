using RobotDomain.Geometry;

namespace MaydayDomain.MotionPlanning;

public interface MaydayMotionPlanner
{
    MaydayStructureSet<MaydayLegPosture> GetPosture();
    void SetPosture(MaydayStructurePosture posture);
    MaydayStructureSet<Xyz> GetTipPositions();

}