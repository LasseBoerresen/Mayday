namespace Mayday.Structures;

public class MaydayStructure(IEnumerable<Joint> joints) : Structure(joints)
{
    public static MaydayStructure Create()
    {
        Joint[] joints = { };
        Attachment[] attachments = { };
        Link[] links = { };

        return new(joints);
    }

    private MaydayPosture _posture = MaydayPosture.Neutral;

    public MaydayPosture Posture
    {
        get => _posture;
        set
        {
            _posture = value;
            Console.WriteLine(_posture);
        }
    }
};
