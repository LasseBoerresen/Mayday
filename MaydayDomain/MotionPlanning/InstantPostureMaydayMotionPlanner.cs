using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public class InstantPostureMaydayMotionPlanner(MaydayStructure structure) : MaydayMotionPlanner
{
    public MaydayStructureSet<MaydayLegPosture> GetPosture() => structure.GetPosture();

    public void SetPosture(MaydayStructurePosture posture) => structure.SetPosture(posture);
    
    public void SetPosture(MaydayLegPosture posture) => SetPosture(MaydayStructurePosture.FromSingle(posture));

    public MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName) => structure.GetPositionsOf(linkName);

    public MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName) => structure.GetOrientationsOf(linkName);
}