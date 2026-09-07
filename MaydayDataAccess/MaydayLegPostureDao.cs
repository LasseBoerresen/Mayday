using MaydayDomain;

namespace MaydayDataAccess;

internal readonly record struct MaydayLegPostureDao(
    double CoxaAngle, 
    double FemurAngle, 
    double TibiaAngle)
{
    public MaydayLegPosture ToDomain() => MaydayLegPosture.FromRevolutions(CoxaAngle, FemurAngle, TibiaAngle);

    public static MaydayLegPostureDao FromDomain(MaydayLegPosture legPosture)
    {
        return new MaydayLegPostureDao(
                legPosture.CoxaAngle.Revolutions, 
                legPosture.FemurAngle.Revolutions, 
                legPosture.TibiaAngle.Revolutions);
    }
}
