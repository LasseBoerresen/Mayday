using System.Numerics;

namespace RobotDomain.Geometry;

public static class Vector3Extensions
{
    public static string ToShortString(this Vector3 v)
    {
        return $"X: {v.X ,6:F3}, Y: {v.Y,6:F3}, Z: {v.Z,6:F3}";
    }
}
