using UnitsNet;
using static System.Math;
using static UnitsNet.RotationalAcceleration;

namespace Generic;

public static class UnitsNetExtensions
{
    public static RotationalAcceleration RotationalAccelerationFromRpm2(double rpmSquared)
    {
        const double secondsPerMinute = 60;
        
        return FromRadiansPerSecondSquared(rpmSquared / Pow(secondsPerMinute, 2.0));
    }
    
    public static bool IsAlmostEqualSingleRotation(Angle angle, Angle other, Angle precision)
    {
        return Abs(Sin(angle.Radians) - Sin(other.Radians)) < Sin(precision.Radians) 
            &&  Abs(Cos(angle.Radians) - Cos(other.Radians)) < Sin(precision.Radians);
    }
    
    public static bool IsAlmostEqual(Length length, Length other, Length precision)
    {
        return Abs((length - other).Meters) < precision.Meters;
    }
    
    /// <Remarks>
    /// Remainder, i.e. '%' on floats gave precision problems, therefore calculating residual manually
    /// </Remarks>
    public static Length Modulo(Length a, Length b)
    {
        return a - b * Floor(a / b);
    }
}
