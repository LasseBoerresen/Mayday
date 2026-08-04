using RobotDomain.Motion;
using RobotDomain.Time;
using UnitsNet;

namespace EllieMain.MotionPlanning;

// TODO probably extract a general robotics motion planner. Some things are not ellie specific, but shared with Mayday. 
public interface EllieMotionPlanner : TrackingMotionPlanner<Motion>
{
    void Start();

    void AccelerateBy(Timed<Speed> timedSpeed);

    void TurnBy(Timed<RotationalSpeed> timedRotationalSpeed);
}
