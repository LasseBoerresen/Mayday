namespace Domain.Structures;

public class MaydayLeg : Structure
{
    private MaydayLegId Id { get; init; }

    private MaydayLeg(IEnumerable<Joint> joints, IEnumerable<Attachment> attachments, IEnumerable<Link> links)
        : base(joints, attachments, links)
    {
    }

    public static MaydayLeg Create(MaydayLegId id)
    {
        return new MaydayLeg(id);
    }

    public static IEnumerable<MaydayLeg> CreateAll()
    {

        return MaydayLegId.All.Select(Create);
    }


}
