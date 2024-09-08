using UnitsNet;

namespace Dynamixel;

internal static class StepSpeed
{
    static readonly RotationalSpeed StepSize = RotationalSpeed.FromRevolutionsPerMinute(0.229);
    
    public static RotationalSpeed ToSpeed(uint steps) => steps * StepSize;
}