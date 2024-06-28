using Generic;
using LanguageExt;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class AdapterSdkImpl(PortAdapter portAdapter) : Adapter
{
    static int DXL_BROADCAST_ID = 254;
    
    static RotationalSpeed _VELOCITY_STEP_SIZE = RotationalSpeed.FromRevolutionsPerMinute(0.229);
    static RotationalAcceleration _ACCELERATION_STEP_SIZE = UnitsNetExtensions.RotationalAccelerationFromRpm2(214.577);
    static Ratio _LOAD_STEP_SIZE = Ratio.FromDecimalFractions(0.1);
    static TemperatureDelta _TEMPERATURE_STEP_SIZE = TemperatureDelta.FromDegreesCelsius(1.0);
    static Ratio _TORQ_LIMIT_REST = Ratio.FromDecimalFractions(1.0);
    static Option<RotationalSpeed> _VEL_LIMIT_SLOW = Option<RotationalSpeed>.None;  // AngularVelocity(tau / 8)  // tau / 16;
    static Option<RotationalAcceleration> _ACC_LIMIT_SLOW = Option<RotationalAcceleration>.None;  // tau / 8;
    static int _POSITION_P_GAIN_SOFT = 200; // 640;  // 200;
    static int _POSITION_I_GAIN_SOFT = 300;
    static int _POSITION_D_GAIN_SOFT = 4000;

    public void SetGoal(JointId id, PositionAngle angle)
    {
        portAdapter.Write(Id.FromJointId(id), ControlRegister.GoalPosition, angle.ToPositionStep());
    }

    public void Initialize(JointId id)
    {
        TorqueEnable(id);
    }

    void TorqueEnable(JointId id)
    {
        portAdapter.Write(Id.FromJointId(id), ControlRegister.TorqueEnable, Convert.ToUInt32(true));
    }
}
