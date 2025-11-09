using JetBrains.Annotations;
using LanguageExt;
using ManualBehavior;
using Test.Utilities;

namespace Test.Integration.Main;

[TestSubject(typeof(MaydayRobot))]
public class MaydayRobotTests
{
    [PhysicalRobotFact]
    void GivenMayWithTerminalPostureBehaviorController_WhenStartThenSleepThenStop_ThenSucceeds()
    {
        // Given
        using var may = MaydayRobot.CreateWithTerminalPostureBehaviorController().RunUnsafe();

        // When
        Task.Run(() => may.Start());
        
        Thread.Sleep(TimeSpan.FromSeconds(0.5));

        may.Stop();

        // Then
        // Succeeds
    }
    
    [PhysicalRobotFact]
    void GivenMayWithBabyLegsBehaviorController_WhenStartThenSleepThenStop_ThenSucceeds()
    {
        // Given
        using var may = MaydayRobot.CreateWithBabyLegsBehaviorController().RunUnsafe();

        // When
        Task.Run(() => may.Start());
        
        Thread.Sleep(TimeSpan.FromSeconds(0.5));

        may.Stop();

        // Then
        // Succeeds
    }
    
    [PhysicalRobotFact]
    void ShouldBeAbleToCreateAndStopAndCreateNew()
    {
        // Given
        // Nothing

        // When
        using (var may0 = MaydayRobot.CreateWithTerminalPostureBehaviorController().RunUnsafe())
        {
            may0.Stop();
        }

        using (var may1 = MaydayRobot.CreateWithTerminalPostureBehaviorController().RunUnsafe())
        {
            may1.Stop();
        }

        // Then
        // Succeeds
    }
}
