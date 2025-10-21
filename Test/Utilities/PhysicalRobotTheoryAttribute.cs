using Xunit;

namespace Test.Utilities;

public sealed class PhysicalRobotTheoryAttribute : TheoryAttribute
{
    public PhysicalRobotTheoryAttribute()
    {
        if (!TestConfiguration.IsRobotConnected())
            Skip = TestConfiguration.MaydayRobotIsNotConnectedReason;
    }
}
