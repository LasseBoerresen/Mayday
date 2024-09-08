using UnitsNet;
using UnitsNet.Units;

namespace Dynamixel;

public record StepAngle(Angle Value)
{
    public static readonly uint StepCenter = 2048;
    static readonly uint StepExtreme = 2047; // TODO create PostitionStep class to encapsulate 
    static readonly Angle StepSize = Angle.FromRadians(Math.Tau / (StepExtreme * 2));

    public static StepAngle FromRevs(double value) => new(Angle.FromRevolutions(value));

    public static StepAngle FromPositionStep(uint value)
    {
        return new(((int)value - StepCenter) * StepSize.ToUnit(AngleUnit.Revolution));
    }

    public uint ToPositionStep()
    {
        ThrowIfNotWithinSemiCircle(Value);
        return (uint)(Value  / StepSize ) + StepCenter;
    }

    void ThrowIfNotWithinSemiCircle(Angle angle)
    {
        if (angle < Angle.FromRevolutions(-0.5) || angle >= Angle.FromRevolutions(0.5))
            throw new ArgumentException($"Angle bigger than a semicircle, got: '{angle}'");
    }
}
