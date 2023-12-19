using mayday.Geometry;

namespace mayday.Structures;

public class Structure(IEnumerable<Joint> joints, IEnumerable<Attachment> attachments, IEnumerable<Link> links)
{
    public IEnumerable<JointState> JointStates => joints.Select(j => j.State);

    public Pose GetPoseFor(Component component)
    {
        // TODO fix cheat and actually do forward kinematics
        return component.Origin;
    }
}
