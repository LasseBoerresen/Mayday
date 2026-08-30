using EllieMain.MotionPlanning;
using RobotDomain.Time;
using UnitsNet;

namespace EllieMain.Structures;

public interface EllieStructure
{
    /// <summary>
    /// Start physical devices. 
    /// </summary>
    void Start(CancellationToken ct);

    void MoveAt(Timed<StructureSet<Speed, Angle>> timedMotions);

    State GetState();
}
