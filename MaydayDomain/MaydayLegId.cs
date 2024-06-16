namespace MaydayDomain;

public record MaydayLegId(Side Side, SidePosition SidePosition) : IComparable<MaydayLegId>
{
    public static IEnumerable<MaydayLegId> AllLegIds =>
        [RightFront, RightCenter, RightRear, LeftFront, LeftCenter, LeftRear];
        
    public static readonly MaydayLegId RightFront = new(Side.Right, SidePosition.Front);
    public static readonly MaydayLegId RightCenter = new(Side.Right, SidePosition.Center);
    public static readonly MaydayLegId RightRear = new(Side.Right, SidePosition.Rear);
    public static readonly MaydayLegId LeftFront = new(Side.Left, SidePosition.Front);
    public static readonly MaydayLegId LeftCenter = new(Side.Left, SidePosition.Center);
    public static readonly MaydayLegId LeftRear = new(Side.Left, SidePosition.Rear);

    private int ComparisonValue => (int)Side * Enum.GetValues(typeof(SidePosition)).Length + (int)SidePosition;
    
    public int CompareTo(MaydayLegId? other)
    {
        if (other == null)
            return 1;
            
        return ComparisonValue - other.ComparisonValue;
    }
}
