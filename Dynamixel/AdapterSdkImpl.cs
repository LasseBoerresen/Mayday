using Generic;
using LanguageExt;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class AdapterSdkImpl(PortAdapter portAdapter) : Adapter
{
    static int DXL_BROADCAST_ID = 254;
    
    static RotationalAcceleration _ACCELERATION_STEP_SIZE = UnitsNetExtensions.RotationalAccelerationFromRpm2(214.577);
    static Ratio _LOAD_STEP_SIZE = Ratio.FromDecimalFractions(0.1);
    static TemperatureDelta _TEMPERATURE_STEP_SIZE = TemperatureDelta.FromDegreesCelsius(1.0);
    static Ratio _TORQ_LIMIT_REST = Ratio.FromDecimalFractions(1.0);
    static Option<RotationalSpeed> _velocityLimitSlow = RotationalSpeed.FromRevolutionsPerSecond(0.5);  // AngularVelocity(tau / 8)  // tau / 16;
    static Option<RotationalAcceleration> _ACC_LIMIT_SLOW = Option<RotationalAcceleration>.None;  // tau / 8;
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
        throw new NotImplementedException($"{nameof(AdapterSdkImpl)}.{nameof(GetState)} is not implemented");
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

    public void SetGoal(JointId id, PositionAngle angle)
    {
        portAdapter.Write(id, ControlRegister.GoalPosition, angle.ToPositionStep());
    }

    void SetVelocityLimit(JointId id)
    {
        var dynamixelVelocity = _velocityLimitSlow
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