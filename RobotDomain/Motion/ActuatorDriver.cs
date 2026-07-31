using RobotDomain.Physics;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace RobotDomain.Motion;

// TODO Use ActuatorId instead of joint Id, because it is also for wheels, and
//  it works better as dynamixel id 

/// <summary>
/// General actuator initialization and control functionality for single or
/// multiple actuators.   
/// </summary>
public interface ActuatorDriver
{
    void Initialize(ActuatorId id, RotationDirection rotationDirection);

    IDictionary<ActuatorId, Angle> ReadAngles(IEnumerable<ActuatorId> ids);

    void SetGoalAngles(IReadOnlyDictionary<ActuatorId, Angle> goalAnglesByIdMap);

    Angle ReadAngle(ActuatorId id);

    LoadRatio ReadLoadRatio(ActuatorId id);

    Temperature ReadTemperature(ActuatorId id);

    Angle ReadAngleGoal(ActuatorId id);

    RotationalSpeed ReadSpeed(ActuatorId id);

    void RotateAt(ActuatorId id, RotationalSpeed speed);
}
