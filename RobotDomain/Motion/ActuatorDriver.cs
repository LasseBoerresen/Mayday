using RobotDomain.Physics;
using RobotDomain.Structures;
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
    void Initialize(JointId id, RotationDirection rotationDirection);

    IDictionary<JointId, Angle> ReadAngles(IEnumerable<JointId> ids);

    void SetGoalAngles(IReadOnlyDictionary<JointId, Angle> goalAnglesByIdMap);

    Angle ReadAngle(JointId id);

    LoadRatio ReadLoadRatio(JointId id);

    Temperature ReadTemperature(JointId id);

    Angle ReadAngleGoal(JointId id);

    RotationalSpeed ReadSpeed(JointId id);
}
