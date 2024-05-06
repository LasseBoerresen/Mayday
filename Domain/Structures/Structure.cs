using System.Collections;
using Domain.Geometry;

namespace Domain.Structures;

public abstract class Structure
{
    public IEnumerable<Joint> Joints { get; init; }
    public IEnumerable<Attachment> Attachments { get; init; }
    public IEnumerable<Link> Links { get; init; }

    protected Structure(IEnumerable<Joint> joints, IEnumerable<Attachment> attachments, IEnumerable<Link> links)
    {
        Joints = joints;
        Attachments = attachments;
        Links = links;
    }

    public IEnumerable<JointState> JointStates => Joints.Select(j => j.State);

    public Pose GetPoseFor(Component component)
    {

        // TODO fix cheat and actually do forward kinematics
        return component.Origin;
    }
}
