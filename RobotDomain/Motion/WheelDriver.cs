using RobotDomain.Time;
using UnitsNet;

namespace RobotDomain.Motion;

public interface WheelDriver
{
    // void RotateAt(WheelId id, Timed<Speed> goalSpeedTimed);
    void Initialize(WheelId id, RobotDomain.Structures.RotationDirection rotationDirection);
    // WheelState GetStateFor(WheelId id);
    void RotateAt(WheelId id, Timed<RotationalSpeed> speed);
}