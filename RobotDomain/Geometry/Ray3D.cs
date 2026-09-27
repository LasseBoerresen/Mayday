using System.Numerics;
using UnitsNet;

namespace RobotDomain.Geometry;

public readonly record struct Ray3D(Xyz Origin, Xyz Direction)
{
    public Length DistanceToPlane(Plane plane)
    {
        var rayDir = Direction.AsVector3Meters();
        var denom = Vector3.Dot(plane.Normal, rayDir);

        // const float epsilon = 1e-6f;
        // // Ray is parallel (or nearly parallel) to the plane
        // if (MathF.Abs(denom) < epsilon)
        // {
        //     return null;
        // }

        var signedDistOrigin = Plane.DotCoordinate(plane, Origin.AsVector3Meters());
        var t = -signedDistOrigin / denom;

        // // Optional: if only forward ray intersections are valid
        // if (t < 0)
        // {
        //     return null; // Plane is behind the ray
        // }

        return Length.FromMeters(t);
    }
    
    public Ray3 ToNumerics()
    {
        return new Ray3(Origin.AsVector3Meters(), Direction.AsVector3Meters());
    }
}