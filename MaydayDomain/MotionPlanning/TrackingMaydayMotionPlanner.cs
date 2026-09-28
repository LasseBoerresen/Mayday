using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Motion;
using RobotDomain.Structures;
using RobotDomain.Time;
using Duration = UnitsNet.Duration;

namespace MaydayDomain.MotionPlanning;

/// <summary>
/// Periodically applies the current target of a motion plan to a Mayday structure.
/// </summary>
/// <remarks>
/// The current implementation tracks thorax lean. Motion plans are sampled at
/// a fixed interval; movements that require nonlinear actuator subgoals are
/// not yet split into intermediate steps.
/// </remarks>
public class TrackingMaydayMotionPlanner
    : MaydayMotionPlanner
{
    // TODO make or run unit test that guarantees that goal sets plan correctly. 
    public Option<Timed<Motion>> Goal
    {
        get;
        set
        {
            field = value;
            
            // TODO make better steps than just the single goal value. Start at
            //   current state, and split in duration steps. 
            Plan = value.Map(MotionPlan<Motion> (tg) => new LinearStepsMotionPlan<Motion>([tg]));
        }
    } = Option<Timed<Motion>>.None;

    public Option<MotionPlan<Motion>> Plan { get; private set; } = Option<MotionPlan<Motion>>.None;
    
    protected readonly MaydayStructure Structure;
    readonly TimeProvider _timeProvider;
    Task? _trackingTask;
    readonly Duration Period = Duration.FromSeconds(0.2);

    public TrackingMaydayMotionPlanner(
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
            "This tracking motion planner does not support moving tip positions.");
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

    void TrackGoalOnce(MotionPlan<Motion> goalMotionTimed)
    {
        // TODO actually split up the movement, so the structure does not just 
        //  receive the final goal, because the different actuators should not
        //  move there at the same pace, it depends on the non-linearity of the
        //  leg pose versus the goal thorax pose.  
    
        // Note: To start with, only the thorax lean is tracked, because the
        // other movement components require stepping.
        // Console.WriteLine("MoveThoraxTo: " + goalMotionTimed.Target.Lean.Xyz);

        Structure.MoveThoraxTo(goalMotionTimed.At(_timeProvider.GetUtcNow()));
    }
}