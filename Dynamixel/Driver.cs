using Generic;
using LanguageExt;
using RobotDomain.Motion;
using RobotDomain.Physics;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace Dynamixel;

/// <summary>
/// General dynamixel initialization and control with regular units   
/// </summary>
/// <remarks>
/// Handles reboot on error at startup
/// </remarks>
/// <param name="_communicationBus"></param>
public class Driver(CommunicationBus _communicationBus) : ActuatorDriver
{
    // Velocity limit cannot be high or infinite, otherwise the motors will
    // reset on big movements, perhaps because of voltage drop. 0.5 rev/s seems to work. 
    static readonly Option<RotationalSpeed> VelocityLimitSlow = RotationalSpeed.FromRevolutionsPerSecond(0.5);  // AngularVelocity(tau / 8)  // tau / 16;
    
    // TODO use PID values or remove them!
    static uint _POSITION_P_GAIN_SOFT = 500; // 640;  // 200;
    static uint _POSITION_I_GAIN_SOFT = 2000;
    static uint _POSITION_D_GAIN_SOFT = 4000;

    public void Initialize(JointId id, RobotDomain.Structures.RotationDirection rotationDirection)
    {
        if(!Ping(id))
            Reboot(id);

        ReadHardwareErrorStatus(id);

        TorqueDisable(id);
        SetVelocityLimit(id);
        SetRotationDirection(id, rotationDirection);
        SetPIDGains(id);
        SetReturnDelay(id);
        TorqueEnable(id);
    }

    public IDictionary<JointId, Angle> ReadAngles(IEnumerable<JointId> ids)
    {
        var dynamixelIds = ids.Select(id => (Id)id);
    
        var positionStepsById = _communicationBus.Read(dynamixelIds, ControlRegister.PresentPosition);

        return positionStepsById
            .Map(kvp => ((JointId)kvp.Key, StepAngle.ToAngle(kvp.Value)))
            .ToDictionary();
    }

    public void SetGoalAngles(IReadOnlyDictionary<JointId, Angle> goalAnglesByIdMap)
    {
        var goalAngleStepsByIdMap = goalAnglesByIdMap.ToDictionary(
                kvp => (Id)kvp.Key,
                kvp => StepAngle.ToSteps(kvp.Value));
    
        _communicationBus.Write(goalAngleStepsByIdMap, ControlRegister.GoalPosition);
    }

    public Angle ReadAngle(JointId id)
    {
        var positionSteps = _communicationBus.Read(Id.FromBase(id), ControlRegister.PresentPosition);

        var angle = StepAngle.ToAngle(positionSteps);
        
        // Console.WriteLine("new Angle: " + angle);
        return angle;
    }

    public LoadRatio ReadLoadRatio(JointId id)
    {
        var loadSteps = _communicationBus.Read(Id.FromBase(id), ControlRegister.PresentLoad);
        
        // TODO Test with real dynamixels, that -1000:1000 range is converted correctly, from uint to int...
        return LoadRatio.FromSteps((int)loadSteps);
    }

    public UnitsNet.Temperature ReadTemperature(JointId id)
    {
        var temperatureSteps = _communicationBus.Read(Id.FromBase(id), ControlRegister.PresentTemperature);

        return StepTemperature.ToTemperature(temperatureSteps);
    }

    public Angle ReadAngleGoal(JointId id)
    {
        var positionSteps = _communicationBus.Read(Id.FromBase(id), ControlRegister.GoalPosition);
        
        return StepAngle.ToAngle(positionSteps);
    }

    public RotationalSpeed ReadSpeed(JointId id)
    {
        var speedSteps = _communicationBus.Read(Id.FromBase(id), ControlRegister.PresentVelocity);
        
        return StepSpeed.ToSpeed(speedSteps);
    }

    void ReadHardwareErrorStatus(JointId id)
    {
        var hardwareErrorStatus = _communicationBus.Read(Id.FromBase(id), ControlRegister.HardwareErrorStatus);
        if (hardwareErrorStatus != 0)
            Console.WriteLine($"HardwareErrorStatus: {hardwareErrorStatus:b8}");
    }

    void Reboot(JointId id)
    {
        Console.WriteLine($"Rebooting {id}");
        _communicationBus.Reboot(Id.FromBase(id));
        Thread.Sleep(300);
        
        var delay = TimeSpan.FromSeconds(0.1);
        var maxDelay = TimeSpan.FromSeconds(5);
        while (Ping(id) != true && delay < maxDelay)
        {   
            Console.WriteLine($"ping failed, ping again in {delay}");
            Thread.Sleep(delay);
            delay *= 2;
        }
    }

    bool Ping(JointId id)
    {
        return _communicationBus.Ping(Id.FromBase(id));
    }

    void SetVelocityLimit(JointId id)
    {
        var dynamixelVelocity = VelocityLimitSlow
                .Map(DynamixelRotationalSpeed.FromRotationalSpeed)
                .IfNone(DynamixelRotationalSpeed.Infinite);
         
        _communicationBus.Write(Id.FromBase(id), ControlRegister.ProfileVelocity, dynamixelVelocity.Value);
    }

    void SetPIDGains(JointId id)
    {
        _communicationBus.Write(Id.FromBase(id), ControlRegister.PositionPGain, _POSITION_P_GAIN_SOFT);
        _communicationBus.Write(Id.FromBase(id), ControlRegister.PositionIGain, _POSITION_I_GAIN_SOFT);
        _communicationBus.Write(Id.FromBase(id), ControlRegister.PositionDGain, _POSITION_D_GAIN_SOFT);
    }
    
    void SetReturnDelay(JointId id)
    {
        // must be low, i.e. 0us or 2us for fast robot communicatoin without latency 
        _communicationBus.Write(Id.FromBase(id), ControlRegister.ReturnDelayTime, 0);
    }

    void SetRotationDirection(JointId id, RobotDomain.Structures.RotationDirection rotationDirection)
    {
        var driveMode = GetDriveMode(id);
        var driveModeUpdated = driveMode & RotationDirection.FromDomain(rotationDirection).Value;
        SetDriveMode(id, driveModeUpdated);
    }

    uint GetDriveMode(JointId id)
    {
        return _communicationBus.Read(Id.FromBase(id), ControlRegister.DriveMode);
    }

    void SetDriveMode(JointId id, uint driveMode)
    {
        _communicationBus.Write(Id.FromBase(id), ControlRegister.DriveMode, driveMode);
    }

    void TorqueEnable(JointId id) => SetTorque(id, true);

    void TorqueDisable(JointId id) => SetTorque(id, false);

    void SetTorque(JointId id, bool value)
    {
        _communicationBus.Write(Id.FromBase(id), ControlRegister.TorqueEnable, Convert.ToUInt32(value));
    }
}
