using RobotDomain.Time;
using UnitsNet;

namespace RobotDomain.Motion;

public class PeriodicallyBatchedWheelDriver(ActuatorDriver actuatorDriver) : WheelDriver 
{
    public void Initialize(WheelId id, Structures.RotationDirection rotationDirection)
    {
        actuatorDriver.Initialize(id, rotationDirection);
    }

    public void RotateAt(WheelId frontLeft, Timed<RotationalSpeed> speed)
    {
        throw new NotImplementedException();
    }
}
