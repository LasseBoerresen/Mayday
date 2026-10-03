using Xunit;

namespace Test.Unit;

public class RedTestCheck
{
    [Fact]
    public void ThisTestFailsOnPurpose() => Assert.Fail("Deliberate failure to verify the gate goes red.");
}
