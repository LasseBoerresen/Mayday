using RobotDomain.Motion;
using RobotDomain.Time;
using UnitsNet;

namespace EllieMain.MotionPlanning;

public interface EllieMotionPlanner : MotionPlanner<Motion>
{
    void AccelerateBy(Timed<Speed> timedSpeedChange);

    void TurnBy(Timed<RotationalSpeed> timedRotationalSpeedChange);
    
    void AccelerateTo(Timed<Speed> timedSpeed);

    void TurnAt(Timed<RotationalSpeed> timedRotationalSpeed);
}
