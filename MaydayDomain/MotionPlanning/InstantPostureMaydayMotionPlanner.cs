using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public class InstantPostureMaydayMotionPlanner(MaydayStructure structure) : MaydayMotionPlanner
{
    public override MaydayStructurePosture GetPosture() => 
        throw new NotImplementedException();

    public override void SetPosture(MaydayStructurePosture posture) => 
        structure.SetPosture(posture);
}