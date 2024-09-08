using UnitsNet;

namespace RobotDomain.Physics;

public record LoadRatio(Ratio Value)
{
    public static readonly LoadRatio Zero = new(Ratio.Zero);
    static readonly Ratio StepSize = Ratio.FromPercent(0.1);

    public static LoadRatio FromSteps(int steps) => new(steps * StepSize);
}
