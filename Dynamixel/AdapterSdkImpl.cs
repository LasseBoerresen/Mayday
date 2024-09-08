using Generic;
using LanguageExt;
using RobotDomain.Physics;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class AdapterSdkImpl(PortAdapter portAdapter) : Adapter
{
    static int DXL_BROADCAST_ID = 254;
    
    static readonly Option<RotationalSpeed> VelocityLimitSlow = RotationalSpeed.FromRevolutionsPerSecond(0.5);  // AngularVelocity(tau / 8)  // tau / 16;
    static int _POSITION_P_GAIN_SOFT = 200; // 640;  // 200;
    static int _POSITION_I_GAIN_SOFT = 300;
    static int _POSITION_D_GAIN_SOFT = 4000;

    public void Initialize(JointId id)
    {
        if(!Ping(id))
            Reboot(id);
        
        ReadHardwareErrorStatus(id);
        
        TorqueDisable(id);
        SetVelocityLimit(id);
        TorqueEnable(id);
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
        var positionSteps = portAdapter.Read(id, ControlRegister.PresentPosition);
        
        return StepAngle.ToAngle(positionSteps);
    }

    Angle ReadAngleGoal(JointId id)
    {
        var positionSteps = portAdapter.Read(id, ControlRegister.GoalPosition);
        
        return StepAngle.ToAngle(positionSteps);
    }

    RotationalSpeed ReadSpeed(JointId id)
    {
        var speedSteps = portAdapter.Read(id, ControlRegister.PresentVelocity);
        
        return StepSpeed.ToSpeed(speedSteps);
    }

    LoadRatio ReadLoadRatio(JointId id)
    {
        var loadSteps = portAdapter.Read(id, ControlRegister.PresentLoad);
        
        // TODO Test with real dynamixels, that -1000:1000 range is converted correctly, from uint to int...
        return LoadRatio.FromSteps((int)loadSteps);
    }
    
    UnitsNet.Temperature ReadTemperature(JointId id)
    {
        var temperatureSteps = portAdapter.Read(id, ControlRegister.PresentTemperature);

        return StepTemperature.ToTemperature(temperatureSteps);
    }
    
    void ReadHardwareErrorStatus(JointId id)
    {
        var hardwareErrorStatus = portAdapter.Read(id, ControlRegister.HardwareErrorStatus);
        if (hardwareErrorStatus != 0)
            Console.WriteLine($"HadwareErrorStatus: {hardwareErrorStatus:b8}");
    }

    void Reboot(JointId id)
    {
        Console.WriteLine($"Rebooting {id}");
        portAdapter.Reboot(id);
        Thread.Sleep(300);
        
        while (Ping(id) != true)
        {   
            Console.WriteLine("ping failed, ping again in 100ms");
            Thread.Sleep(100);    
        };
        
    }

    bool Ping(JointId id)
    {
        return portAdapter.Ping(id);
    }

    public void SetGoal(JointId id, Angle angle)
    {
        portAdapter.Write(id, ControlRegister.GoalPosition, StepAngle.ToSteps(angle));
    }

    void SetVelocityLimit(JointId id)
    {
        var dynamixelVelocity = VelocityLimitSlow
            .Map(DynamixelRotationalSpeed.FromRotationalSpeed)
            .IfNone(DynamixelRotationalSpeed.Infinite);
         
        portAdapter.Write(id, ControlRegister.ProfileVelocity, dynamixelVelocity.Value);
    }

    void TorqueEnable(JointId id) => SetTorque(id, true);

    void TorqueDisable(JointId id) => SetTorque(id, false);

    void SetTorque(JointId id, bool value)
    {
        portAdapter.Write(id, ControlRegister.TorqueEnable, Convert.ToUInt32(value));
    }
}