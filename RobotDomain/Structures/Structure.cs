using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public class Structure()
{

    public Pose GetPoseFor(Component component)
    {

        // TODO fix cheat and actually do forward kinematics
        return component.Origin;
    }
}
