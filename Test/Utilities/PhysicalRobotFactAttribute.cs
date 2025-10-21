namespace Test.Utilities;
using Xunit;

public sealed class PhysicalRobotFactAttribute : FactAttribute
{
    public PhysicalRobotFactAttribute()
    {
        if (!TestConfiguration.IsRobotConnected())
            Skip = TestConfiguration.MaydayRobotIsNotConnectedReason;
    }
}