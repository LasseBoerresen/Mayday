using EllieMain.MotionPlanning;
using RobotDomain.Motion;
using RobotDomain.Time;
using UnitsNet;

namespace EllieMain.Structures;

/// <summary>
/// Represents the physical robot and is responsible for  communicating with it
/// and taking into account structural dimensions such as wheel size and
/// gearing etc. 
/// </summary> 
public class DefaultEllieStructure(WheelDriver wheelDriver) : EllieStructure
{
    bool isStarted = false;
    public void Start(CancellationToken ct)
    {
        // Avoid double starting. 
        if (isStarted)
            return; 
        isStarted = true;
        
        wheelDriver.Initialize(WheelId.FrontLeft, RobotDomain.Structures.RotationDirection.Reverse);
        wheelDriver.Initialize(WheelId.FrontRight, RobotDomain.Structures.RotationDirection.Forward);        
    }

    public void MoveAt(Timed<StructureSet<Speed, Angle>> timedMotions)
    {
        throw new NotImplementedException();
    }

    public State GetState()
    {
        throw new NotImplementedException();
    }
}