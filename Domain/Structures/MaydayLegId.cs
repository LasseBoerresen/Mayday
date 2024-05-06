namespace Domain.Structures;

public readonly record struct MaydayLegId
{
    private string Id { get; init; }

    MaydayLegId(string id) => Id = id;

    public static MaydayLegId RightFront()  => new("rf");
    public static MaydayLegId RightCenter() => new("rc");
    public static MaydayLegId RightBack()   => new("rb");
    public static MaydayLegId LeftFront()   => new("lf");
    public static MaydayLegId LeftCenter()  => new("lc");
    public static MaydayLegId LeftBack()    => new("lb");
    public static IEnumerable<MaydayLegId> All = new[]
    {
        RightFront(), RightCenter(), RightBack(), LeftFront(), LeftCenter(), LeftBack()
    };

}
