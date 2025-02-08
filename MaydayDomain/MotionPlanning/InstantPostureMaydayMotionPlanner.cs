using Dynamixel;
using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public class InstantPostureMaydayMotionPlanner(MaydayStructure structure) : MaydayMotionPlanner
{
    public MaydayStructureSet<MaydayLegPosture> GetPosture() => structure.GetPosture();
    
    public virtual void SetTipPositions(MaydayStructureSet<Xyz> tipPositions)
    {
        throw new NotSupportedException(
            "This naive motion planner does not support setting tip positions, only joint angles.");
    }

    public void SetPosture(MaydayStructurePosture posture) => structure.SetPosture(posture);
    
    public void SetPosture(MaydayLegPosture posture) => SetPosture(MaydayStructurePosture.FromSingle(posture));

    public MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName) => structure.GetPositionsOf(linkName);

    public MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName) => structure.GetOrientationsOf(linkName);
    
    public MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName) => structure.GetTransformsOf(linkName);

    public static InstantPostureMaydayMotionPlanner Create()
    {
        MaydayStructure structure = CreateMaydayStructure();

        InstantPostureMaydayMotionPlanner maydayMotionPlanner = new(structure);
        return maydayMotionPlanner;
    }

    protected static MaydayStructure CreateMaydayStructure()
    {
        JointFactory jointFactory = DynamixelJointFactory.CreateWithDynamixelJoints();

        var structure = MaydayStructure.Create(jointFactory);
        return structure;
    }
}