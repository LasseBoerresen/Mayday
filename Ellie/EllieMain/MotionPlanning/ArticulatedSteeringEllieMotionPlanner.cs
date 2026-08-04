using RobotDomain.Geometry;
using RobotDomain.Motion;
using RobotDomain.Time;
using UnitsNet;

namespace EllieMain.MotionPlanning;

// TODO Extract a abstract generic TrackedMotionPlanner which holds a TMovement and continuousply tracks it. 

public class ArticulatedSteeringEllieMotionPlanner(WheelDriver wheelDriver) 
    : PeriodicTrackingMotionPlanner<Motion>, EllieMotionPlanner
{
    public void Start()
    {
        wheelDriver.Initialize(WheelId.FrontLeft, RobotDomain.Structures.RotationDirection.Reverse);
        wheelDriver.Initialize(WheelId.FrontRight, RobotDomain.Structures.RotationDirection.Forward);
    }

    /// <summary>
    /// Accelerate the robot by the given speed from its current speed
    /// </summary>
    /// <param name="timedSpeed"></param>
    public void AccelerateBy(Timed<Speed> timedSpeed)
    {
        Goal = timedSpeed.Map(newSpeed => 
            Goal
                .Some(goal => goal.Target with {ForwardSpeed = newSpeed })
                .None(new Motion(newSpeed, RotationalSpeed.Zero)));
    }

    public void TurnBy(Timed<RotationalSpeed> timedRotationalSpeed)
    {
        throw new NotImplementedException();
    }

    protected override void TrackGoalOnce(Timed<Motion> goalMotionTimed)
    {
        throw new NotImplementedException();
    }
}