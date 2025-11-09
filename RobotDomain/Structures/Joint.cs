using UnitsNet;

namespace RobotDomain.Structures;

public abstract class Joint : Connection, IDisposable
{
    protected Joint(ComponentId id, Link parent, Link child) 
        : base(id, parent, child)
    {
    }
    
    public abstract JointState State { get; }
    
    public abstract void SetAngleGoal(Angle goal);

    public abstract void Dispose();
}



