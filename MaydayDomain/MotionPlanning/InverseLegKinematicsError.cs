using RobotDomain.Geometry;
using UnitsNet;

namespace MaydayDomain.MotionPlanning;

public record InverseLegKinematicsError(double Value)
{
    public static InverseLegKinematicsError From(Xyz expected, Xyz actual)
    {
       return new((expected - actual).Length.Meters);
    }
};
