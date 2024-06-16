using RobotDomain.Structures;

namespace MaydayDomain;

public class MaydayStructure(IEnumerable<Joint> joints) : Structure(joints)
{
    public static MaydayStructure Create()
    {
        Joint[] joints = { };
        Attachment[] attachments = { };
        Link[] links = { };

        return new(joints);
    }

    private Posture _posture = MaydayPosture.Neutral;

    public Posture Posture
    {
        get => _posture;
        set
        {
            _posture = value;
            Console.WriteLine(_posture);
        }
    }
};
