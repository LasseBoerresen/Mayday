namespace Domain.Structures;

public class MaydayStructure : Structure
{
    public IEnumerable<MaydayLeg> Legs { get; init; }

    private MaydayStructure(IEnumerable<MaydayLeg> legs)
        : base(
            legs.SelectMany(l => l.Joints),
            legs.SelectMany(l => l.Attachments),
            legs.SelectMany(l => l.Links))
    {
        Legs = legs;
    }

    public static MaydayStructure Create()
    {
        var legs = MaydayLegId.All().Map(MaydayLeg.Create());
        return new MaydayStructure([], [], []);
    }

    MaydayPosture _posture = MaydayPosture.Neutral;


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
