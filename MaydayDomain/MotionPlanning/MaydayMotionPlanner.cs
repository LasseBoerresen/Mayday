namespace MaydayDomain.MotionPlanning;

public abstract class MaydayMotionPlanner
{
    public abstract MaydayStructurePosture GetPosture();
    public abstract void SetPosture(MaydayStructurePosture posture);
}