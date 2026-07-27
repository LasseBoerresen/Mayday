using System.Diagnostics;
using Generic;
using RobotDomain.Motion;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace Dynamixel;

public class PeriodicallyScheduledJointDriver : JointDriver
{
    readonly Driver _driver;
    readonly JointStateCache _jointStateCache;
    readonly CancellationTokenSource _cancellationTokenSource;
    readonly TimeProvider _timeProvider;
    readonly Task _setGoalAngleTask;
    readonly TimeSpan _setGoalAnglePeriod = TimeSpan.FromMilliseconds(10);

    public PeriodicallyScheduledJointDriver(
        Driver driver,
        JointStateCache jointStateCache,
        CancellationTokenSource cancellationTokenSource,
        TimeProvider timeProvider)
    {
        _driver = driver;
        _jointStateCache = jointStateCache;
        _cancellationTokenSource = cancellationTokenSource;
        _timeProvider = timeProvider;

        _setGoalAngleTask = PeriodicScheduler.RunAsync(SetGoalAngles, _setGoalAnglePeriod, cancellationTokenSource.Token);
    }

    void UpdateJointAngleCache()
    {
        _driver.ReadAngles(GetInitializedDynamixelIds())
            .ForEach(kvp => _jointStateCache.SetAngleFor(kvp.Key, kvp.Value));
    }

    // TODO write a group-based version, for faster robot startup 
    public void Initialize(JointId id, RobotDomain.Structures.RotationDirection rotationDirection)
    {
        _driver.Initialize(id, rotationDirection);
        
        // Ensure a state value is always available post initialization. 
        _jointStateCache.SetFor(id, _driver.GetInitialJointState(id));
    }

    public JointState GetState(JointId id)
    {
        return _jointStateCache.GetFor(id);
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
        
        _communicationBus.Write(interpolatedGoalAnglesByIdAsDynamixel, ControlRegister.GoalPosition);
    }

    double InterpolatedStepFactor<T>(Timed<T> timed)
    {
        return timed.StepFactor(currentTime: _timeProvider.GetUtcNow());
    }

    public void SetGoalAngleFor(JointId id, Timed<Angle> goalAngleTimed)
    {
        _jointStateCache.SetAngleGoalFor(id, goalAngleTimed);
    }

    public void Dispose()
    {
        _communicationBus.Dispose();
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