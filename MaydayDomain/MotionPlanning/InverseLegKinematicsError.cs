using RobotDomain.Geometry;
using UnitsNet;

namespace MaydayDomain.MotionPlanning;

public record InverseLegKinematicsError(Length Value)
{
    public static InverseLegKinematicsError From(Xyz expected, Xyz actual)
    {
       return new((expected - actual).Length);
    }
};
