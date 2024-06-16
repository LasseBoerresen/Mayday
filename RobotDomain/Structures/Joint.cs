using UnitsNet;

namespace RobotDomain.Structures;

public abstract class Joint

{
    public abstract JointState State { get; }
    public abstract void SetAngleGoal(Angle goal);
}
