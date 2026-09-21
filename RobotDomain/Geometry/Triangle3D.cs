using RobotDomain.Geometry.SystemNumerics;
using UnitsNet;

namespace RobotDomain.Geometry;

public readonly record struct Triangle3D(Xyz v0, Xyz v1, Xyz v2)
{
    public (Xyz? Point, Length? Distance) LookForIntersectionWith(Ray3D ray)
    {
        if (ToNumerics().TryGetIntersection(ray.ToNumerics(), out var intersectionPoint, out var distance))
            return (Point: Xyz.FromVector3Meters(intersectionPoint), Distance: Length.FromMeters(distance));
        
        return (Point: null, Distance: null);
    }

    Triangle3 ToNumerics()
    {
        return new(
            v0.AsVector3Meters(),
            v1.AsVector3Meters(),
            v2.AsVector3Meters());
    }
}
