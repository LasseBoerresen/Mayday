using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public class InstantPostureMaydayMotionPlanner(MaydayStructure structure) : MaydayMotionPlanner
{
    public override MaydayStructurePosture GetPosture() => 
        structure.GetPosture();

    public override void SetPosture(MaydayStructurePosture posture) => 
        structure.SetPosture(posture);
}