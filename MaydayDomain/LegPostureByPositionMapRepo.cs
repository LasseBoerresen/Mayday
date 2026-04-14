using LanguageExt;
using RobotDomain.Geometry;

namespace MaydayDomain;

public interface LegPostureByPositionMapRepo
{
    LegPostureByPositionMap Load();
    
    void Store(LegPostureByPositionMap map);
}