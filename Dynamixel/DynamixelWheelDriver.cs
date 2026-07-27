using RobotDomain.Motion;

namespace Dynamixel;

public class DynamixelWheelDriver(Driver _driver) : WheelDriver 
{
    public void Initialize(WheelId id, RobotDomain.Structures.RotationDirection rotationDirection)
    {
        throw new NotImplementedException("Dynamixel wheel controller not implemented yet.");
    }
}
