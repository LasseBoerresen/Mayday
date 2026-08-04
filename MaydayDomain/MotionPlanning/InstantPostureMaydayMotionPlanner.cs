using RobotDomain.Geometry;
using RobotDomain.Motion;
using RobotDomain.Structures;
using RobotDomain.Time;

namespace MaydayDomain.MotionPlanning;

// TODO rename Mayday.Movement into motion, because it is used in a MotionPlanner.
public class InstantPostureMaydayMotionPlanner 
    : PeriodicTrackingMotionPlanner<Motion>, MaydayMotionPlanner
{
    protected readonly MaydayStructure Structure;

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

    protected override void TrackGoalOnce(Timed<Motion> goalMotionTimed)
    {
        // TODO actually split up the movement, so the structure does not just 
        //  receive the final goal, because the different actuators should not
        //  move there at the same pace, it depends on the non-linearity of the
        //  leg pose versus the goal thorax pose.  
    
        // Note: To start with, only the thorax lean is tracked, because the
        // other movement components require stepping.
        Console.WriteLine("MoveThoraxTo: " + goalMotionTimed.Target.Lean.Xyz);
        
        Structure.MoveThoraxTo(goalMotionTimed.Map(m => m.Lean));
    }
}