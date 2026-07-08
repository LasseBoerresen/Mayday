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
    Task? _trackingTask;
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
        Structure.MoveTipsTo(tipPositionsTimed);
    }

    public MaydayLegPosture GetPosture(MaydayLegId legId) => Structure.GetPostureOf(legId);

    public void SetPosture(Timed<MaydayStructurePosture> postureTimed) => Structure.SetPosture(postureTimed);
    
    public void SetPosture(Timed<MaydayLegPosture> postureTimed) 
        => SetPosture(postureTimed.Map(MaydayStructurePosture.FromSingle));

    public MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName) => Structure.GetPositionsOf(linkName);

    public Xyz GetPositionOf(LinkName linkName, MaydayLegId legId) => Structure.GetPositionOf(linkName, legId);

    public MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName) => Structure.GetOrientationsOf(linkName);
    
    public MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName) => Structure.GetTransformsOf(linkName);

    public void Start(CancellationToken ct)
    {
        _trackingTask = PeriodicScheduler.RunAsync(
            action: () => _goalMovement.IfSome(TrackGoalOnce), 
            duration: Duration.FromSeconds(1), 
            ct);
    }

    void TrackGoalOnce(Timed<Movement> goalMovementTimed)
    {
        // Note: To start with, only the thorax lean is tracked, because the
        // other movement components require stepping.
        Console.WriteLine("MoveThoraxTo: " + goalMovementTimed.Target.Lean.Xyz);
        Structure.MoveThoraxTo(goalMovementTimed.Map(m => m.Lean));
    }

    public Option<Timed<Movement>> GetGoal() => _goalMovement;
    
    public void SetGoal(Timed<Movement> movementTimed) => _goalMovement = movementTimed;

    public void UnsetGoal() => _goalMovement = Option<Timed<Movement>>.None;

    public static InstantPostureMaydayMotionPlanner Create(MaydayLegFactory legFactory)
    {
        var structure = CreateMaydayStructure(legFactory);
        
        var maydayMotionPlanner = new InstantPostureMaydayMotionPlanner(structure);
        return maydayMotionPlanner;
    }

    protected static DefaultMaydayStructure CreateMaydayStructure(MaydayLegFactory legFactory)
    {
        var structure = DefaultMaydayStructure.Create(legFactory);
        return structure;
    }
    
    public void Dispose()
    {
        _trackingTask?.Dispose();
    }
}