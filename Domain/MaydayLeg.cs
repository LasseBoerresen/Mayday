using System.Collections.Immutable;
using Domain.Structures;

namespace Domain;

public class MaydayLeg
{
    private IEnumerable<Joint> _joints;
    
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
}