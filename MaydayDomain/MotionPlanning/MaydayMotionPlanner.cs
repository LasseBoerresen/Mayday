using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public interface MaydayMotionPlanner
{
    MaydayStructureSet<MaydayLegPosture> GetPostures();
    MaydayLegPosture GetPostureOf(MaydayLegId legId);
    void MoveTipPositions(MaydayStructureSet<Xyz> tipDeltas);
    void SetTipPositionsForLegs(MaydayStructureSet<Xyz> tipPositions);
    void SetPosture(MaydayStructurePosture posture);
    void SetPosture(MaydayLegPosture posture);
    MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName);
    Xyz GetPositionOf(LinkName linkName, MaydayLegId legId);
    MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName);
    MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName);
    
    /// <summary>
    /// Start tracking the goal 
    /// </summary>
    Task Start(CancellationToken ct);
    
    /// <summary>
    /// Gets the current goal 
    /// </summary>
    Option<Movement> GetGoal();
    
    /// <summary>
    /// Sets goal for the motion planner to continuously pursue 
    /// </summary>
    /// <param name="movement"></param>
    void SetGoal(Movement movement);
    
    /// <summary>
    /// Removes the movement goal, effectively pausing the motion planner.  
    /// </summary>
    void UnsetGoal();
}