using RobotDomain.Time;
using UnitsNet;

namespace RobotDomain.Motion;

public interface WheelController
{
    // void RotateAt(WheelId id, Timed<Speed> goalSpeedTimed);
    void Initialize(WheelId id, RobotDomain.Structures.RotationDirection rotationDirection);
    // WheelState GetStateFor(WheelId id);
}