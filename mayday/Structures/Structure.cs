namespace mayday.Structures;

public class Structure(IEnumerable<Joint> joints)
{
    public IEnumerable<JointState> JointStates => joints.Select(j => j.State);
}
