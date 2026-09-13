using System.Numerics;
using Generic;

namespace RobotDomain.Geometry;

public static class Vector3Extensions
{
    public static Vector3 Random(float max = 1.0f)
    {
        Random random = new();
        
        return new(
            x: max * (float)random.NextDoubleNeg1ToPos1(), 
            y: max * (float)random.NextDoubleNeg1ToPos1(), 
            z: max * (float)random.NextDoubleNeg1ToPos1());
    }
    
    public static string ToShortString(this Vector3 v)
    {
        return $"X: {v.X ,6:F3}, Y: {v.Y,6:F3}, Z: {v.Z,6:F3}";
    }
}
