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
    Option<Timed<Movement>> _goalMovement = Option<Timed<Movement>>.None;


    public InstantPostureMaydayMotionPlanner(MaydayStructure structure)
    {
        Structure = structure;
    }

    public MaydayStructureSet<MaydayLegPosture> GetPostures() => Structure.GetPostures();

    public MaydayLegPosture GetPostureOf(MaydayLegId legId) => Structure.GetPostureOf(legId);
    
    public virtual void MoveTipPositions(Timed<MaydayStructureSet<Xyz>> tipDeltasTimed)
    {
        throw new NotSupportedException(
            "This naive motion planner does not support Moving tip positions, only setting joint angles.");
    }

    public void SetTipPositionsForLegs(Timed<MaydayStructureSet<Xyz>> tipPositionsTimed)
    {
        Structure.MoveTipsTo(tipPositionsTimed, CancellationToken.None);
    }

    public MaydayLegPosture GetPosture(MaydayLegId legId) => Structure.GetPostureOf(legId);

    public void SetPosture(Timed<MaydayStructurePosture> postureTimed) => Structure.SetPosture(postureTimed);
    
    public void SetPosture(Timed<MaydayLegPosture> postureTimed) 
        => SetPosture(postureTimed.Map(MaydayStructurePosture.FromSingle));

    public MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName) => Structure.GetPositionsOf(linkName);

    public Xyz GetPositionOf(LinkName linkName, MaydayLegId legId) => Structure.GetPositionOf(linkName, legId);

    public MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName) => Structure.GetOrientationsOf(linkName);
    
    public MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName) => Structure.GetTransformsOf(linkName);

    public Task Start(CancellationToken ct)
    {
        return PeriodicScheduler.RunAsync(
            action: () => _goalMovement.IfSome(gm => TrackGoalOnce(gm, ct)), 
            duration: Duration.FromSeconds(0.1), 
            ct);
    }

    void TrackGoalOnce(Timed<Movement> goalMovementTimed, CancellationToken ct)
    {
        // Note: To start with, only the thorax lean is tracked, because the
        // other movement components require stepping.
        
        Structure.MoveThoraxTo(goalMovementTimed.Map(m => m.Lean), ct);
    }

    public Option<Timed<Movement>> GetGoal() => _goalMovement;
    
    public void SetGoal(Timed<Movement> movementTimed) => _goalMovement = movementTimed;

    public void UnsetGoal() => _goalMovement = Option<Timed<Movement>>.None;

    public static Eff<InstantPostureMaydayMotionPlanner> Create(
        CancellationTokenSource cancellationTokenSource,
        TimeProvider timeProvider)
    {
        var structureEff = CreateMaydayStructure(cancellationTokenSource, timeProvider);
        var maydayMotionPlanner = structureEff.Map(structure => new InstantPostureMaydayMotionPlanner(structure));
        
        return maydayMotionPlanner;
    }

    protected static Eff<MaydayStructure> CreateMaydayStructure(
        CancellationTokenSource cancellationTokenSource,
        TimeProvider timeProvider)
    {
        var jointFactoryEff = DynamixelJointFactory.Create(cancellationTokenSource, timeProvider);

        var structure = jointFactoryEff.Map(MaydayStructure.Create);
        return structure;
    }
}