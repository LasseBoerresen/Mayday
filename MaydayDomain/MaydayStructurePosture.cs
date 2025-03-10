﻿namespace MaydayDomain;

public record MaydayStructurePosture(
    MaydayLegPosture RF,
    MaydayLegPosture RC,
    MaydayLegPosture RB,
    MaydayLegPosture LF,
    MaydayLegPosture LC,
    MaydayLegPosture LB) 
    : MaydayStructureSet<MaydayLegPosture>(RF, RC, RB, LF, LC, LB)
{
    public static MaydayStructurePosture FromSingle(MaydayLegPosture legPosture)
    {
        return new(legPosture, legPosture, legPosture, legPosture, legPosture, legPosture);
    }

    public static MaydayStructurePosture FromSet(MaydayStructureSet<MaydayLegPosture> legPostures)
    {
        return new(legPostures.RF, legPostures.RC, legPostures.RB, legPostures.LF, legPostures.LC, legPostures.LB);
    }
}
