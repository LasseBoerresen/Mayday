using RobotDomain.Geometry;

namespace MaydayDomain.MotionPlanning;

public class InstantPostureMaydayMotionPlanner(MaydayStructure structure) : MaydayMotionPlanner
{
    public MaydayStructureSet<MaydayLegPosture> GetPosture() => structure.GetPosture();

    public void SetPosture(MaydayStructurePosture posture) => structure.SetPosture(posture);

    public MaydayStructureSet<Xyz> GetTipPositions() => structure.GetTipPoints();
}