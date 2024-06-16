using Domain.Geometry;

namespace Domain.Structures;

public class Structure(IEnumerable<Joint> joints)
{
    public IEnumerable<JointState> JointStates => joints.Select(j => j.State);

    public Pose GetPoseFor(Component component)
    {

        // TODO fix cheat and actually do forward kinematics
        return component.Origin;
    }
}
