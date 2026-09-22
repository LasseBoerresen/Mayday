using UnitsNet;

namespace RobotDomain.Geometry;

public readonly record struct Ray3DIntersection(Xyz Point, Length Distance);
