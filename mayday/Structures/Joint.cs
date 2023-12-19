namespace mayday.Structures;

public class Joint(Motor motor, Link? parent=null, Link? child=null) : Connection(parent, child)
{
    public JointState State => motor.State;
}
