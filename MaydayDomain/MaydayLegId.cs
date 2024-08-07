﻿using RobotDomain.Structures;

namespace MaydayDomain;

public record MaydayLegId(Side Side, SidePosition SidePosition) : IComparable<MaydayLegId>
{
    public static IEnumerable<MaydayLegId> AllLegIds =>
        [LeftFront, LeftCenter, LeftBack, RightFront, RightCenter, RightBack];

    public static readonly MaydayLegId LeftFront = new(Side.Left, SidePosition.Front);
    public static readonly MaydayLegId LeftCenter = new(Side.Left, SidePosition.Center);
    public static readonly MaydayLegId LeftBack = new(Side.Left, SidePosition.Rear);
    public static readonly MaydayLegId RightFront = new(Side.Right, SidePosition.Front);
    public static readonly MaydayLegId RightCenter = new(Side.Right, SidePosition.Center);
    public static readonly MaydayLegId RightBack = new(Side.Right, SidePosition.Rear);

    public int Value() => 
        (int)Side * NumberOfSidePositions() * MaydayLeg.JointCount
        + (int)SidePosition * MaydayLeg.JointCount;

    public int CompareTo(MaydayLegId? other)
    {
        if (other == null)
            return 1;

        return Value() - other.Value();
    }

    public JointId JointId(int jointNumber) => new(Value() + jointNumber);

    static int NumberOfSidePositions()
    {
        return Enum.GetValues(typeof(SidePosition)).Length;
    }
}
