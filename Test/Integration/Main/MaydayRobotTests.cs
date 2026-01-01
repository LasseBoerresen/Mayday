using JetBrains.Annotations;
using LanguageExt;
using ManualBehavior;
using Test.Utilities;
using Xunit;

namespace Test.Integration.Main;

[TestSubject(typeof(MaydayRobot))]
public class MaydayRobotTests
{
    static readonly TimeProvider TimeProvider = TimeProvider.System;
    
    [PhysicalRobotFact]
    void GivenMayWithTerminalPostureBehaviorController_WhenStartThenSleepThenStop_ThenSucceeds()
    {
        // Given
        var may = MaydayRobot.CreateWithTerminalPostureBehaviorController(TimeProvider).RunUnsafe();

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
        var may = MaydayRobot.CreateWithBabyLegsBehaviorController(TimeProvider).RunUnsafe();

        // When
        Task.Run(() => may.Start());
        
        Thread.Sleep(TimeSpan.FromSeconds(0.5));

        may.Stop();

        // Then
        // Succeeds
    }
}
