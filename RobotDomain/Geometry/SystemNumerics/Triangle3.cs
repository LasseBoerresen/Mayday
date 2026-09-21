using System.Numerics;
using System.Runtime.CompilerServices;

namespace RobotDomain.Geometry.SystemNumerics;

public readonly record struct Triangle3(Vector3 v0, Vector3 v1, Vector3 v2)
{
    // High machine precision epsilon for critical robotics tolerances
    const double EPSILON = 1e-9;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetIntersection(
        in Ray3 ray, 
        out Vector3 intersectionPoint,
        out float distance)
    {
        intersectionPoint = default;
        distance = 0;

        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;
        Vector3 pvec = Vector3.Cross(ray.Direction, edge2);
        var det = Vector3.Dot(edge1, pvec);

        // In robotics, backface culling is usually disabled (det near zero check instead of det < EPSILON)
        // We need to know if we hit an obstacle regardless of which side the sensor approaches from.
        if (Math.Abs(det) < EPSILON) 
            return false;
    
        var invDet = 1.0f / det;
        Vector3 tvec = ray.Origin - v0;

        var u = Vector3.Dot(tvec, pvec) * invDet;
        if (u < 0.0 || u > 1.0) 
            return false;

        Vector3 qvec = Vector3.Cross(tvec, edge1);

        var v = Vector3.Dot(ray.Direction, qvec) * invDet;
        if (v < 0.0 || u + v > 1.0) 
            return false;

        var t = Vector3.Dot(edge2, qvec) * invDet;

        if (t > EPSILON) 
        {
            distance = t;
            intersectionPoint = ray.Origin + (ray.Direction * t);
            return true;
        }

        return false;
    }
}
