using Dynamixel;
using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;

using Duration = UnitsNet.Duration;

namespace MaydayDomain.MotionPlanning;

public class InstantPostureMaydayMotionPlanner : MaydayMotionPlanner
{
    protected readonly MaydayStructure Structure;
    PeriodicScheduler _scheduler;
    Option<Movement> _goalMovement = Option<Movement>.None;


    public InstantPostureMaydayMotionPlanner(
        MaydayStructure structure,
        PeriodicScheduler scheduler)
    {
        Structure = structure;
        _scheduler = scheduler;
    }

    public MaydayStructureSet<MaydayLegPosture> GetPostures() => Structure.GetPostures();

    public MaydayLegPosture GetPostureOf(MaydayLegId legId) => Structure.GetPostureOf(legId);
    
    public virtual void MoveTipPositions(MaydayStructureSet<Xyz> tipDeltas)
    {
        throw new NotSupportedException(
            "This naive motion planner does not support Moving tip positions, only setting joint angles.");
    }

    public void SetTipPositionsForLegs(MaydayStructureSet<Xyz> tipPositions)
    {
        Structure.MoveTipsTo(tipPositions, Duration.FromSeconds(1.0), CancellationToken.None);
    }

    public MaydayLegPosture GetPosture(MaydayLegId legId) => Structure.GetPostureOf(legId);

    public void SetPosture(MaydayStructurePosture posture) => Structure.SetPosture(posture);
    
    public void SetPosture(MaydayLegPosture posture) => SetPosture(MaydayStructurePosture.FromSingle(posture));

    public MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName) => Structure.GetPositionsOf(linkName);

    public Xyz GetPositionOf(LinkName linkName, MaydayLegId legId) => Structure.GetPositionOf(linkName, legId);

    public MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName) => Structure.GetOrientationsOf(linkName);
    
    public MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName) => Structure.GetTransformsOf(linkName);

    public Task Start(CancellationToken ct)
    {
        return _scheduler.RunAsync(() => 
            _goalMovement.IfSome(gm => TrackGoalOnce(gm, ct)), ct);
    }

    void TrackGoalOnce(Movement goalMovement, CancellationToken ct)
    {
        // Note: To start with, only the thorax lean is tracked, because the
        // other movement components require stepping.
        
        Structure.MoveThoraxTo(goalMovement.Lean, goalMovement.Duration, ct);
    }

    public Option<Movement> GetGoal() => _goalMovement;
    
    public void SetGoal(Movement movement) => _goalMovement = movement;

    public void UnsetGoal() => _goalMovement = Option<Movement>.None;

    public static Eff<InstantPostureMaydayMotionPlanner> Create(CancellationTokenSource cancellationTokenSource)
    {
        var structureEff = CreateMaydayStructure(cancellationTokenSource);
        PeriodicScheduler scheduler = new(TimeProvider.System, UnitsNet.Duration.FromSeconds(0.1));

        var maydayMotionPlanner = structureEff.Map(structure =>  
            new InstantPostureMaydayMotionPlanner(structure, scheduler));
        
        return maydayMotionPlanner;
    }

    protected static Eff<MaydayStructure> CreateMaydayStructure(CancellationTokenSource cancellationTokenSource)
    {
        var jointFactoryEff = DynamixelJointFactory.Create(cancellationTokenSource);

        var structure = jointFactoryEff.Map(MaydayStructure.Create);
        return structure;
    }
}