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
}
