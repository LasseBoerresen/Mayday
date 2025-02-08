using RobotDomain.Geometry;

namespace MaydayDomain.MotionPlanning;

public record InverseLegKinematicsInput(Xyz EndXyz, Xyz StartXyz, Rpy StartRpy);
