﻿using Generic;
using LanguageExt;
using RobotDomain.Physics;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class AdapterSdkImpl : Adapter
{
    readonly PortAdapter _portAdapter;
    readonly JointStateCache _jointStateCache;

    public AdapterSdkImpl(PortAdapter portAdapter, JointStateCache jointStateCache)
    {
        _portAdapter = portAdapter;
        _jointStateCache = jointStateCache;
    }

    static int DXL_BROADCAST_ID = 254;
    
    static readonly Option<RotationalSpeed> VelocityLimitSlow = RotationalSpeed.FromRevolutionsPerSecond(0.5);  // AngularVelocity(tau / 8)  // tau / 16;
    
    // TODO use PID values or remove them!
    static int _POSITION_P_GAIN_SOFT = 200; // 640;  // 200;
    static int _POSITION_I_GAIN_SOFT = 300;
    static int _POSITION_D_GAIN_SOFT = 4000;

    public void Initialize(JointId id, RobotDomain.Structures.RotationDirection rotationDirection)
    {
        if(!Ping(id))
            Reboot(id);
        
        ReadHardwareErrorStatus(id);
        
        TorqueDisable(id);
        SetVelocityLimit(id);
        SetRotationDirection(id, rotationDirection);
        TorqueEnable(id);
        
        // Ensure a state value is always available post initialization. 
        _jointStateCache.SetFor(id, GetState(id));
    }

    public JointState GetState(JointId id)
    {
        return new JointState(
            ReadAngle(id),
            ReadSpeed(id),
            ReadLoadRatio(id),
            ReadTemperature(id),
            ReadAngleGoal(id));
    }

    Angle ReadAngle(JointId id)
    {
        var positionSteps = _portAdapter.Read(id, ControlRegister.PresentPosition);
        
        return StepAngle.ToAngle(positionSteps);
    }

    Angle ReadAngleGoal(JointId id)
    {
        var positionSteps = _portAdapter.Read(id, ControlRegister.GoalPosition);
        
        return StepAngle.ToAngle(positionSteps);
    }

    RotationalSpeed ReadSpeed(JointId id)
    {
        var speedSteps = _portAdapter.Read(id, ControlRegister.PresentVelocity);
        
        return StepSpeed.ToSpeed(speedSteps);
    }

    LoadRatio ReadLoadRatio(JointId id)
    {
        var loadSteps = _portAdapter.Read(id, ControlRegister.PresentLoad);
        
        // TODO Test with real dynamixels, that -1000:1000 range is converted correctly, from uint to int...
        return LoadRatio.FromSteps((int)loadSteps);
    }

    UnitsNet.Temperature ReadTemperature(JointId id)
    {
        var temperatureSteps = _portAdapter.Read(id, ControlRegister.PresentTemperature);

        return StepTemperature.ToTemperature(temperatureSteps);
    }

    void ReadHardwareErrorStatus(JointId id)
    {
        var hardwareErrorStatus = _portAdapter.Read(id, ControlRegister.HardwareErrorStatus);
        if (hardwareErrorStatus != 0)
            Console.WriteLine($"HardwareErrorStatus: {hardwareErrorStatus:b8}");
    }

    void Reboot(JointId id)
    {
        Console.WriteLine($"Rebooting {id}");
        _portAdapter.Reboot(id);
        Thread.Sleep(300);
        
        var delay = TimeSpan.FromSeconds(0.1);
        while (Ping(id) != true)
        {   
            Console.WriteLine($"ping failed, ping again in {delay}");
            Thread.Sleep(delay);
            delay *= 2;
        };
        
    }

    bool Ping(JointId id)
    {
        return _portAdapter.Ping(id);
    }

    public void SetGoal(JointId id, Angle angle)
    {
        _portAdapter.Write(id, ControlRegister.GoalPosition, StepAngle.ToSteps(angle));
    }

    void SetVelocityLimit(JointId id)
    {
        var dynamixelVelocity = VelocityLimitSlow
            .Map(DynamixelRotationalSpeed.FromRotationalSpeed)
            .IfNone(DynamixelRotationalSpeed.Infinite);
         
        _portAdapter.Write(id, ControlRegister.ProfileVelocity, dynamixelVelocity.Value);
    }

    void SetRotationDirection(JointId id, RobotDomain.Structures.RotationDirection rotationDirection)
    {
        var driveMode = GetDriveMode(id);
        var driveModeUpdated = driveMode & RotationDirection.FromDomain(rotationDirection).Value;
        SetDriveMode(id, driveModeUpdated);
    }
    
    uint GetDriveMode(JointId id)
    {
        return _portAdapter.Read(id, ControlRegister.DriveMode);
    }
    
    void SetDriveMode(JointId id, uint driveMode)
    {
        _portAdapter.Write(id, ControlRegister.DriveMode, driveMode);
    }

    void TorqueEnable(JointId id) => SetTorque(id, true);

    void TorqueDisable(JointId id) => SetTorque(id, false);

    void SetTorque(JointId id, bool value)
    {
        _portAdapter.Write(id, ControlRegister.TorqueEnable, Convert.ToUInt32(value));
    }

    public void Dispose()
    {
        _portAdapter.Dispose();
    }
}