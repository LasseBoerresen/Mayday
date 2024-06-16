using RobotDomain.Structures;

namespace MaydayDomain;

public class MaydayLeg
{
    private readonly IEnumerable<Joint> _joints;
    
    public MaydayLeg(IEnumerable<Joint> joints)
    {
        _joints = joints;
    }

    public MaydayLegPosture GetPosture()
    {
        return new(_joints.Select(j => j.State.Angle));
    }

    public virtual void SetPosture(MaydayLegPosture posture)
    {
        _joints
            .Zip(posture.AsEnumerable())
            .ToList()
            .ForEach(pair => pair.First.SetAngleGoal(pair.Second));
    }
}