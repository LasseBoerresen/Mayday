using UnitsNet;
using UnitsNet.Units;

namespace Dynamixel;

/// <summary>
/// Converts between XL430 position steps and angles. The actuator reports and accepts steps 0 to 4,095, which is
/// 4,096 counts per revolution (0.088 degrees per count), with <see cref="StepCenter"/> as zero angle. Step 0 is
/// therefore -0.5 revolutions and step 4,095 is 0.5 revolutions minus one count.
/// </summary>
/// <remarks>See the Robotis e-Manual, XL430-W250 control table: Goal Position (116) and Present Position (132).</remarks>
public static class StepAngle
{
    public static readonly uint StepCenter = 2048;
    const uint StepsPerRevolution = 4096;
    const uint HighestStep = StepsPerRevolution - 1;
    static readonly Angle StepSize = Angle.FromRevolutions(1.0 / StepsPerRevolution);

    /// <summary>Converts a position step reported by the actuator to its angle from the center step.</summary>
    public static Angle ToAngle(uint steps)
    {
        return ((int)steps - StepCenter) * StepSize.ToUnit(AngleUnit.Revolution);
    }

    /// <summary>
    /// Converts an angle from the center step to the nearest position step the actuator accepts. An angle within
    /// half a count below 0.5 revolutions rounds to the highest step, 4,095, instead of the out-of-range 4,096.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// The angle is outside -0.5 (inclusive) to 0.5 (exclusive) revolutions, which no step represents.
    /// </exception>
    public static uint ToSteps(Angle angle)
    {
        ThrowIfNotWithinSemiCircle(angle);

        // Round, never truncate: angle / StepSize carries floating-point error, so truncating turns step 7 into
        // step 6.
        var step = Math.Round(angle / StepSize) + StepCenter;

        return (uint)Math.Min(step, HighestStep);
    }

    static void ThrowIfNotWithinSemiCircle(Angle angle)
    {
        if (angle < Angle.FromRevolutions(-0.5) || angle >= Angle.FromRevolutions(0.5))
            throw new ArgumentException($"Angle bigger than a semicircle, got: '{angle}'");
    }
}
