using System.Numerics;
using UnitsNet;

namespace RobotDomain.Geometry;

public readonly record struct Ray3D(Xyz Origin, Xyz Direction)
{
    public Length DistanceToPlane(Plane plane)
    {
        // TODO handle potential division by zero for rays parallel to plane. 
        //  for those, check if Vector3.Dot(plane.Normal, rayDirection) < epsilon = 1e-6
        
        // Assumes plane.Normal is normalized.
        var signedDistance = Plane.DotCoordinate(plane, Origin.AsVector3Meters());
        
        return Length.FromMeters(signedDistance);
    }
    
    public Ray3 ToNumerics()
    {
        return new Ray3(Origin.AsVector3Meters(), Direction.AsVector3Meters());
    }
}