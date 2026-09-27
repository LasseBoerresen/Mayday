using System.Numerics;
using RobotDomain.Geometry.SystemNumerics;
using UnitsNet;

namespace RobotDomain.Geometry;

public readonly record struct Triangle3D(Xyz v0, Xyz v1, Xyz v2)
{
    public Area Area() => UnitsNet.Area.FromSquareMeters(ToNumerics().Area());
    
    public Xyz Center() => Xyz.FromVector3Meters(ToNumerics().Center());
    
    public bool IsColinear()
    {
        return ToNumerics().IsColinear();
    }

    public Ray3DIntersection? LookForIntersectionWith(Ray3D ray)
    {
        if (ToNumerics().TryGetIntersection(ray.ToNumerics(), out var intersectionPoint, out var distance))
            return new Ray3DIntersection(
                Point: Xyz.FromVector3Meters(intersectionPoint), 
                Distance: Length.FromMeters(distance));

        return null;
    }

    Triangle3D FromNumerics(Triangle3 triangle) => 
        new(
            Xyz.FromVector3Meters(triangle.v0), 
            Xyz.FromVector3Meters(triangle.v1), 
            Xyz.FromVector3Meters(triangle.v2));
    
    Triangle3 ToNumerics()
    {
        return new(
            v0.AsVector3Meters(),
            v1.AsVector3Meters(),
            v2.AsVector3Meters());
    }

    public static Triangle3D FromList(IReadOnlyList<Xyz> vertices)
    {
        if (vertices.Count != 3)
            throw new ArgumentException("Must have exactly 3 vertices", nameof(vertices));

        return new(vertices[0], vertices[1], vertices[2]);
    }

    public Plane ToPlane()
    {
        return Plane.CreateFromVertices(
            v0.AsVector3Meters(),
            v1.AsVector3Meters(),
            v2.AsVector3Meters());
    }
}
