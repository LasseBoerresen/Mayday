using Generic;
using LanguageExt;
using RobotDomain.Physics;
using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public class AdapterSdkImpl : Adapter
{
    readonly PortAdapter _portAdapter;
    readonly JointStateCache _jointStateCache;
    readonly CancellationTokenSource _cancellationTokenSource;
    readonly Task _updateStateTask;
    readonly Task _updateAngleTask;
    readonly TimeSpan _updateStatePeriod = TimeSpan.FromMilliseconds(500);
    readonly TimeSpan _updateAnglePeriod = TimeSpan.FromMilliseconds(100);

    public AdapterSdkImpl(
        PortAdapter portAdapter,
        JointStateCache jointStateCache,
        CancellationTokenSource cancellationTokenSource)
    {
        _portAdapter = portAdapter;
        _jointStateCache = jointStateCache;
        _cancellationTokenSource = cancellationTokenSource;
        
        _updateStateTask = Task.Run(() => UpdateLoopAsync(UpdateJointStateCache, _updateStatePeriod));
        _updateAngleTask = Task.Run(() => UpdateLoopAsync(UpdateJointAngleCache, _updateAnglePeriod));

    }

    async Task UpdateLoopAsync(Action<JointId> cacheUpdateAction, TimeSpan updatePeriod)
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                foreach (var id in _jointStateCache.GetIds())
                {
                    try
                    {
                        cacheUpdateAction(id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating cache for joint {id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting ids from the cache: {ex.Message}");
            }
            
            try
            {
                await Task.Delay(updatePeriod, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Ignore exception when the task is canceled.
            }
        }
    }

    private void UpdateJointStateCache(JointId id)
    {
        _jointStateCache.SetFor(id, GetNewState(id));
    }

    private void UpdateJointAngleCache(JointId id)
    {
        _jointStateCache.SetAngleFor(id, ReadAngle(id));
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
        _jointStateCache.SetFor(id, GetNewState(id));
    }

    public JointState GetState(JointId id)
    {
        return _jointStateCache.GetFor(id);
    }
    
    JointState GetNewState(JointId id)
    {
        JointState jointState = new JointState(
            ReadAngle(id),
            ReadSpeed(id),
            ReadLoadRatio(id),
            ReadTemperature(id),
            ReadAngleGoal(id));
            
        // Console.WriteLine("new joint state: " + jointState);    
        return jointState;
    }

    Angle ReadAngle(JointId id)
    {
        var positionSteps = _portAdapter.Read(id, ControlRegister.PresentPosition);

        var angle = StepAngle.ToAngle(positionSteps);
        
        // Console.WriteLine("new Angle: " + angle);
        return angle;
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
        CancelAndDisposeUpdateTask();
    }

    void CancelAndDisposeUpdateTask()
    {
        _cancellationTokenSource.Cancel();
        try
        {
            _updateStateTask.Wait();
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
        {
            // Ignore cancellation exceptions.
        }
        
        try
        {
            _updateAngleTask.Wait();
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
        {
            // Ignore cancellation exceptions.
        }
        
        _cancellationTokenSource.Dispose();
    }
}