using UnitsNet;

namespace MaydayDomain;

public record JointLimits(
    Angle CoxaMin,
    Angle CoxaMax,
    Angle FemurMin,
    Angle FemurMax,
    Angle TibiaMin,
    Angle TibiaMax)
{
    /// <remarks>
    /// Ballpark guesstimated angle limits, please fix from 3d model.
    /// </remarks>
    public static JointLimits Defaults = new(
        CoxaMin: Angle.FromRevolutions(-0.25),
        CoxaMax: Angle.FromRevolutions(0.25),
        FemurMin: Angle.FromRevolutions(-0.25),
        FemurMax: Angle.FromRevolutions(0.25),
        TibiaMin: Angle.FromRevolutions(-0.4),
        TibiaMax: Angle.FromRevolutions(0.4));
}
