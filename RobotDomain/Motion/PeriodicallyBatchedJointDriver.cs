using Generic;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;

namespace RobotDomain.Motion;

public class PeriodicallyBatchedJointDriver : JointDriver
{
    readonly ActuatorDriver _driver;
    readonly JointStateCache _jointStateCache;
    readonly CancellationTokenSource _cancellationTokenSource;
    readonly TimeProvider _timeProvider;
    readonly Task _setGoalAngleTask;
    readonly TimeSpan _setGoalAnglePeriod = TimeSpan.FromMilliseconds(10);

    public PeriodicallyBatchedJointDriver(
        ActuatorDriver driver,
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
        _driver.ReadAngles(_jointStateCache.GetIds())
            .ForEach(kvp => _jointStateCache.SetAngleFor(kvp.Key, kvp.Value));
    }

    // TODO write a group-based version, for faster robot startup 
    public void Initialize(JointId id, RotationDirection rotationDirection)
    {
        _driver.Initialize(id, rotationDirection);
        
        // Ensure a state value is always available post initialization. 
        _jointStateCache.SetFor(id, GetInitialJointState(id));
    }

    JointState GetInitialJointState(JointId id)
    {
        var jointState = new JointState(
            _driver.ReadAngle(id),
            _driver.ReadSpeed(id),
            _driver.ReadLoadRatio(id),
            _driver.ReadTemperature(id),
            AngleGoal: Timed<Angle>.Passed(_driver.ReadAngleGoal(id)),
            AngleGoalPrevious: Timed<Angle>.Passed(_driver.ReadAngleGoal(id)));
            
        // Console.WriteLine("new joint state: " + jointState);    
        return jointState;
    }

    public JointState GetState(JointId id)
    {
        return _jointStateCache.GetFor(id);
    }

    void SetGoalAngles()
    {
        // Also update joint angles syncronously, they are needed for inverse kinematics, when finding the closes joints.  
        UpdateJointAngleCache();
        
        var interpolatedGoalAnglesById = _jointStateCache
            .GetById()
            .MapValueToReadonly(jointState => jointState.InterpolateGoalAngleOneTimeStep(InterpolatedStepFactor));
        
        _driver.SetGoalAngles(interpolatedGoalAnglesById);
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