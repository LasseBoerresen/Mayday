using JetBrains.Annotations;
using Main;
using Xunit;

namespace Test.Integration.Main;

[TestSubject(typeof(MaydayRobot))]
public class MaydayRobotTests
{
    [Fact]
    void GivenMay_WhenStartThenSleepThenStop_ThenSucceeds()
    {
        // Given
        var may = MaydayRobot.CreateWithTerminalPostureBehaviorController();

        // When
        Task.Run(() => may.Start());
        
        Thread.Sleep(TimeSpan.FromSeconds(0.5));

        may.Stop();

        // Then
        // Succeeds
    }
}
