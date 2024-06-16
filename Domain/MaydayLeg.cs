using System.Collections;
using System.Collections.Immutable;
using Domain.Structures;

namespace Domain;

public class MaydayLeg
{
    private readonly IEnumerable<Joint> _joints;
    
    public MaydayLeg(IEnumerable<Joint> joints)
    {
        _joints = joints;
    }

    public MaydayLegPosture GetPosture()
    {
        var angles = _joints
            .Select(j => j.State.Angle)
            .ToArray();
        
        return new(angles);
    }

    public void SetPosture(MaydayLegPosture posture)
    {
        _joints
        .Zip(posture.AsEnumerable())
        .ToList()
        .ForEach(pair => pair.First.SetAngleGoal(pair.Second));
    }
}