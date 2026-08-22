using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Motion;
using RobotDomain.Structures;
using RobotDomain.Time;

namespace MaydayDomain.MotionPlanning;

public interface MaydayMotionPlanner : MotionPlanner<Motion>
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
}