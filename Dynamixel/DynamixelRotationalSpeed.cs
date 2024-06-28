using UnitsNet;

namespace Dynamixel;

public record DynamixelRotationalSpeed(uint Value)
{
    public static DynamixelRotationalSpeed Infinite => new(0);
     
    public static readonly RotationalSpeed StepSize = RotationalSpeed.FromRevolutionsPerMinute(0.229);
    
    public static DynamixelRotationalSpeed FromRotationalSpeed(RotationalSpeed value)
    {
        var steps = Math.Ceiling(value / StepSize);
        
        return new((uint) steps);
    }
}