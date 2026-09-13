using UnitsNet;
using UnitsNet.Units;

namespace Dynamixel;

public static class StepAngle
{
    public static readonly uint StepCenter = 2048;
    static readonly uint StepExtreme = 2047; // TODO create PostitionStep class to encapsulate 
    static readonly Angle StepSize = Angle.FromRadians(Math.Tau / (StepExtreme * 2));

    public static Angle ToAngle(uint steps)
    {
        return ((int)steps - StepCenter) * StepSize.ToUnit(AngleUnit.Revolution);
    }

    public static uint ToSteps(Angle angle)
    {
        ThrowIfNotWithinSemiCircle(angle);

        var stepsRelative = angle / StepSize;
        
        return (uint)(stepsRelative + StepCenter);
    }

    static void ThrowIfNotWithinSemiCircle(Angle angle)
    {
        if (angle < Angle.FromRevolutions(-0.5) || angle >= Angle.FromRevolutions(0.5))
            throw new ArgumentException($"Angle bigger than a semicircle, got: '{angle}'");
    }
}
