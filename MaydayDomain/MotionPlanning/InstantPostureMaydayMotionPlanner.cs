using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Motion;
using RobotDomain.Structures;
using RobotDomain.Time;
using Duration = UnitsNet.Duration;

namespace MaydayDomain.MotionPlanning;

/// <summary>
/// adlkfj
/// </summary>
/// <remarks>
/// A tracking motion planner continuously tracks a goal motion of a structure
/// part, which may require nonlinear actuator subgoals.
/// For example, moving a leg tip straight down at a steady pace will require
/// some joints to move more in the beginning than others. Commanding the
/// structure to simply move the tip to the end in one go will mean the tip
/// does not move in a linear fashion towards the goal.
/// </remarks>
public class InstantPostureMaydayMotionPlanner 
    : MaydayMotionPlanner
{
    public Option<Timed<Motion>> Goal { get; set; }
    public Option<MotionPlan<Motion>> Plan { get; }
    protected readonly MaydayStructure Structure;
    readonly TimeProvider _timeProvider;
    Task? _trackingTask;
    readonly Duration Period = Duration.FromSeconds(1);

    public InstantPostureMaydayMotionPlanner(
        MaydayStructure structure, 
        TimeProvider timeProvider)
    {
        Structure = structure;
        _timeProvider = timeProvider;
    }
    
    public void Start(CancellationToken ct)
    {
        Action trackingAction = () => Plan.IfSome(TrackGoalOnce);

        PeriodicScheduler periodicScheduler = new(_timeProvider);
        _trackingTask = periodicScheduler.RunAsync(
                action: trackingAction, 
                duration: Period, 
                ct);
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

    protected void TrackGoalOnce(MotionPlan<Motion> goalMotionTimed)
    {
        // TODO actually split up the movement, so the structure does not just 
        //  receive the final goal, because the different actuators should not
        //  move there at the same pace, it depends on the non-linearity of the
        //  leg pose versus the goal thorax pose.  
    
        // Note: To start with, only the thorax lean is tracked, because the
        // other movement components require stepping.
        // Console.WriteLine("MoveThoraxTo: " + goalMotionTimed.Target.Lean.Xyz);
        
        // Structure.MoveThoraxTo(goalMotionTimed.Map(m => m.Lean));
    }
}