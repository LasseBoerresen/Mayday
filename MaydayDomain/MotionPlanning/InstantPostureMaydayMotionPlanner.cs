using Dynamixel;
using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public class InstantPostureMaydayMotionPlanner : MaydayMotionPlanner
{
    protected readonly MaydayStructure Structure;
    
    public InstantPostureMaydayMotionPlanner(MaydayStructure structure)
    {
        Structure = structure;
    }

    public MaydayStructureSet<MaydayLegPosture> GetPostures() => Structure.GetPostures();

    public MaydayLegPosture GetPostureOf(MaydayLegId legId) => Structure.GetPostureOf(legId);
    
    public virtual void MoveTipPositions(MaydayStructureSet<Xyz> tipDeltas)
    {
        throw new NotSupportedException(
            "This naive motion planner does not support Moving tip positions, only setting joint angles.");
    }

    public MaydayLegPosture GetPosture(MaydayLegId legId) => Structure.GetPostureOf(legId);

    public void SetPosture(MaydayStructurePosture posture) => Structure.SetPosture(posture);
    
    public void SetPosture(MaydayLegPosture posture) => SetPosture(MaydayStructurePosture.FromSingle(posture));

    public MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName) => Structure.GetPositionsOf(linkName);

    public Xyz GetPositionOf(LinkName linkName, MaydayLegId legId) => Structure.GetPositionOf(linkName, legId);

    public MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName) => Structure.GetOrientationsOf(linkName);
    
    public MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName) => Structure.GetTransformsOf(linkName);

    public static Eff<InstantPostureMaydayMotionPlanner> Create(CancellationTokenSource cancellationTokenSource)
    {
        var structureEff = CreateMaydayStructure(cancellationTokenSource);

        var maydayMotionPlanner = structureEff.Map(structure => new InstantPostureMaydayMotionPlanner(structure));
        return maydayMotionPlanner;
    }

    protected static Eff<MaydayStructure> CreateMaydayStructure(CancellationTokenSource cancellationTokenSource)
    {
        var jointFactoryEff = DynamixelJointFactory.Create(cancellationTokenSource);

        var structure = jointFactoryEff.Map(MaydayStructure.Create);
        return structure;
    }
}