using System.Diagnostics;
using Generic;
using LanguageExt;
using RobotDomain.Physics;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace Dynamixel;

public class AdapterSdkImpl : Adapter
{
    readonly PortAdapter _portAdapter;
    readonly JointStateCache _jointStateCache;
    readonly CancellationTokenSource _cancellationTokenSource;
    readonly TimeProvider _timeProvider;
    readonly Task _setGoalAngleTask;
    readonly TimeSpan _setGoalAnglePeriod = TimeSpan.FromMilliseconds(100);

    public AdapterSdkImpl(
        PortAdapter portAdapter,
        JointStateCache jointStateCache,
        CancellationTokenSource cancellationTokenSource,
        TimeProvider timeProvider)
    {
        _portAdapter = portAdapter;
        _jointStateCache = jointStateCache;
        _cancellationTokenSource = cancellationTokenSource;
        _timeProvider = timeProvider;

        _setGoalAngleTask = PeriodicScheduler.RunAsync(SetGoalAngles, _setGoalAnglePeriod, cancellationTokenSource.Token);
    }

    void UpdateJointAngleCache()
    {
        ReadAngles().ForEach(kvp => _jointStateCache.SetAngleFor(kvp.Key, kvp.Value));
    }

    // Velocity limit cannot be high or infinite, otherwise the motors will
    // reset on big movements, perhaps because of voltage drop. 0.5 rev/s seems to work. 
    static readonly Option<RotationalSpeed> VelocityLimitSlow = RotationalSpeed.FromRevolutionsPerSecond(0.5);  // AngularVelocity(tau / 8)  // tau / 16;
    
    // TODO use PID values or remove them!
    static uint _POSITION_P_GAIN_SOFT = 1000; // 640;  // 200;
    static uint _POSITION_I_GAIN_SOFT = 1000;
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
        
        // Ensure a state value is always available post initialization. 
        _jointStateCache.SetFor(id, GetInitialState(id));
    }

    public JointState GetState(JointId id)
    {
        return _jointStateCache.GetFor(id);
    }

    JointState GetInitialState(JointId id)
    {
        var jointState = new JointState(
            ReadAngle(id),
            ReadSpeed(id),
            ReadLoadRatio(id),
            ReadTemperature(id),
            AngleGoal: Timed<Angle>.Passed(ReadAngleGoal(id)),
            AngleGoalPrevious: Timed<Angle>.Passed(ReadAngleGoal(id)));
            
        // Console.WriteLine("new joint state: " + jointState);    
        return jointState;
    }

    Angle ReadAngle(JointId id)
    {
        var positionSteps = _portAdapter.Read(Id.FromBase(id), ControlRegister.PresentPosition);

        var angle = StepAngle.ToAngle(positionSteps);
        
        // Console.WriteLine("new Angle: " + angle);
        return angle;
    }

    IDictionary<JointId, Angle> ReadAngles()
    {
        var positionStepsById = _portAdapter.Read(GetInitializedDynamixelIds(), ControlRegister.PresentPosition);

        return positionStepsById
            .Select(kvp => ((JointId)kvp.Key, StepAngle.ToAngle(kvp.Value)))
            .ToDictionary();
    }

    IEnumerable<Id> GetInitializedDynamixelIds()
    {
        return _jointStateCache.GetIds().Select(Id.FromBase);
    }

    void SetGoalAngles()
    {
        // Also update joint angles syncronously, they are needed for inverse kinematics, when finding the closes joints.  
        UpdateJointAngleCache();
        
        var interpolatedGoalAnglesById = _jointStateCache
            .GetById()
            .MapValueToReadonly(jointState => jointState.InterpolateGoalAngleOneTimeStep(InterpolatedStepFactor));
        
        var interpolatedGoalAnglesByIdAsDynamixel = interpolatedGoalAnglesById.ToDictionary(
            kvp => Id.FromBase(kvp.Key), 
            kvp =>  StepAngle.ToSteps(kvp.Value));
        
        _portAdapter.Write(interpolatedGoalAnglesByIdAsDynamixel, ControlRegister.GoalPosition);
    }

    double InterpolatedStepFactor<T>(Timed<T> timed)
    {
        return timed.StepFactor(currentTime: _timeProvider.GetUtcNow(), timeStep: _setGoalAnglePeriod);
    }

    Angle ReadAngleGoal(JointId id)
    {
        var positionSteps = _portAdapter.Read(Id.FromBase(id), ControlRegister.GoalPosition);
        
        return StepAngle.ToAngle(positionSteps);
    }

    RotationalSpeed ReadSpeed(JointId id)
    {
        var speedSteps = _portAdapter.Read(Id.FromBase(id), ControlRegister.PresentVelocity);
        
        return StepSpeed.ToSpeed(speedSteps);
    }

    LoadRatio ReadLoadRatio(JointId id)
    {
        var loadSteps = _portAdapter.Read(Id.FromBase(id), ControlRegister.PresentLoad);
        
        // TODO Test with real dynamixels, that -1000:1000 range is converted correctly, from uint to int...
        return LoadRatio.FromSteps((int)loadSteps);
    }

    UnitsNet.Temperature ReadTemperature(JointId id)
    {
        var temperatureSteps = _portAdapter.Read(Id.FromBase(id), ControlRegister.PresentTemperature);

        return StepTemperature.ToTemperature(temperatureSteps);
    }

    void ReadHardwareErrorStatus(JointId id)
    {
        var hardwareErrorStatus = _portAdapter.Read(Id.FromBase(id), ControlRegister.HardwareErrorStatus);
        if (hardwareErrorStatus != 0)
            Console.WriteLine($"HardwareErrorStatus: {hardwareErrorStatus:b8}");
    }

    void Reboot(JointId id)
    {
        Console.WriteLine($"Rebooting {id}");
        _portAdapter.Reboot(Id.FromBase(id));
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
        return _portAdapter.Ping(Id.FromBase(id));
    }

    public void SetGoalAngleFor(JointId id, Timed<Angle> goalAngleTimed)
    {
        _jointStateCache.SetAngleGoalFor(id, goalAngleTimed);
    }

    void SetVelocityLimit(JointId id)
    {
        var dynamixelVelocity = VelocityLimitSlow
            .Map(DynamixelRotationalSpeed.FromRotationalSpeed)
            .IfNone(DynamixelRotationalSpeed.Infinite);
         
        _portAdapter.Write(Id.FromBase(id), ControlRegister.ProfileVelocity, dynamixelVelocity.Value);
    }

    void SetPIDGains(JointId id)
    {
        _portAdapter.Write(Id.FromBase(id), ControlRegister.PositionPGain, _POSITION_P_GAIN_SOFT);
        _portAdapter.Write(Id.FromBase(id), ControlRegister.PositionIGain, _POSITION_I_GAIN_SOFT);
        _portAdapter.Write(Id.FromBase(id), ControlRegister.PositionDGain, _POSITION_D_GAIN_SOFT);
    }
    
    void SetReturnDelay(JointId id)
    {
        // must be low, i.e. 0us or 2us for fast robot communicatoin without latency 
        _portAdapter.Write(Id.FromBase(id), ControlRegister.ReturnDelayTime, 0);
    }

    void SetRotationDirection(JointId id, RobotDomain.Structures.RotationDirection rotationDirection)
    {
        var driveMode = GetDriveMode(id);
        var driveModeUpdated = driveMode & RotationDirection.FromDomain(rotationDirection).Value;
        SetDriveMode(id, driveModeUpdated);
    }

    uint GetDriveMode(JointId id)
    {
        return _portAdapter.Read(Id.FromBase(id), ControlRegister.DriveMode);
    }

    void SetDriveMode(JointId id, uint driveMode)
    {
        _portAdapter.Write(Id.FromBase(id), ControlRegister.DriveMode, driveMode);
    }

    void TorqueEnable(JointId id) => SetTorque(id, true);

    void TorqueDisable(JointId id) => SetTorque(id, false);

    void SetTorque(JointId id, bool value)
    {
        _portAdapter.Write(Id.FromBase(id), ControlRegister.TorqueEnable, Convert.ToUInt32(value));
    }

    public void Dispose()
    {
        _portAdapter.Dispose();
        CancelAndDisposeUpdateTask();
    }

    void CancelAndDisposeUpdateTask()
    {
        _cancellationTokenSource.Cancel();
        
        
        try
        {
            _setGoalAngleTask.Wait();
        }
        catch (AggregateException ex)
        {
            // If it's JUST a cancellation, we can ignore it
            if (ex.InnerExceptions.All(e => e is TaskCanceledException))
                return;
                
            // If there were other errors (like hardware disconnects), rethrow!
            throw; 
        }
        
        _cancellationTokenSource.Dispose();
    }
}