using UnitsNet;

namespace RobotDomain.Physics;

// TODO: load ratio seems to be either 0 or ~6000 %, which seems wrong. 
public record LoadRatio(Ratio Value)
{
    public static readonly LoadRatio Zero = new(Ratio.Zero);
    static readonly Ratio StepSize = Ratio.FromPercent(0.1);

    public static LoadRatio FromSteps(int steps) => new(steps * StepSize);
}
