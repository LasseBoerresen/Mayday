using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;

namespace MaydayDomain.MotionPlanning;

public interface MaydayMotionPlanner : IDisposable
{
    MaydayStructureSet<MaydayLegPosture> GetPostures();
    MaydayLegPosture GetPostureOf(MaydayLegId legId);
    void MoveTipPositions(Timed<MaydayStructureSet<Xyz>> tipDeltasTimed);
    void SetTipPositionsForLegs(Timed<MaydayStructureSet<Xyz>> tipPositionsTimed);
    void SetPosture(Timed<MaydayStructurePosture> postureTimed);
    void SetPosture(Timed<MaydayLegPosture> postureTimed);
    MaydayStructureSet<Xyz> GetPositionsOf(LinkName linkName);
    Xyz GetPositionOf(LinkName linkName, MaydayLegId legId);
    MaydayStructureSet<Q> GetOrientationsOf(LinkName linkName);
    MaydayStructureSet<Transform> GetTransformsOf(LinkName linkName);
    
    /// <summary>
    /// Start tracking the goal 
    /// </summary>
    void Start(CancellationToken ct);
    
    /// <summary>
    /// Gets the current goal 
    /// </summary>
    Option<Timed<Movement>> GetGoal();
    
    /// <summary>
    /// Sets goal for the motion planner to continuously pursue 
    /// </summary>
    /// <param name="movementTimed"></param>
    void SetGoal(Timed<Movement> movementTimed);
    
    /// <summary>
    /// Removes the movement goal, effectively pausing the motion planner.  
    /// </summary>
    void UnsetGoal();
}