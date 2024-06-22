using UnitsNet;

namespace Dynamixel;

public record PositionAngle(Angle Value)
{
    public static readonly uint StepCenter = 2048;
    static readonly uint StepExtreme = 2047; // TODO create PostitionStep class to encapsulate 
    static readonly Angle StepAngle = Angle.FromRadians(Math.Tau / (StepExtreme * 2));
    
    public uint ToPositionStep()
    {
        ThrowIfNotWithinSemiCircle(Value);
        return (uint)(Value  / StepAngle ) + StepCenter;
    }

    void ThrowIfNotWithinSemiCircle(Angle angle)
    {
        if (angle < Angle.FromRevolutions(-0.5) || angle >= Angle.FromRevolutions(0.5))
            throw new ArgumentException($"Angle bigger than a semicircle, got: '{angle}'");
    }
}
