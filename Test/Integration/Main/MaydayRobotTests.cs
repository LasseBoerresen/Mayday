﻿using JetBrains.Annotations;
using Main;
using Xunit;

namespace Test.Integration.Main;

[TestSubject(typeof(MaydayRobot))]
public class MaydayRobotTests
{
    [Fact] // [Fact(Skip="robot not connected")]
    void GivenMayWithTerminalPostureBehaviorController_WhenStartThenSleepThenStop_ThenSucceeds()
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
    
    [Fact] // [Fact(Skip="robot not connected")]
    void GivenMayWithBabyLegsBehaviorController_WhenStartThenSleepThenStop_ThenSucceeds()
    {
        // Given
        var may = MaydayRobot.CreateWithBabyLegsBehaviorController();

        // When
        Task.Run(() => may.Start());
        
        Thread.Sleep(TimeSpan.FromSeconds(0.5));

        may.Stop();

        // Then
        // Succeeds
    }
}
