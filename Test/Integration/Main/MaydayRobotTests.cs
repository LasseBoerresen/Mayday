using Generic.System;
using JetBrains.Annotations;
using LanguageExt;
using Robots;
using Robots.Base;
using Test.Integration.Robots;
using Test.Integration.Robots.Base;
using Test.Utilities;

namespace Test.Integration.Main;

[TestSubject(typeof(MaydayRobot))]
public class MaydayRobotTests
{
    static readonly Terminal Terminal = new TestTerminal([]);
    static readonly TimeProvider TimeProvider = TimeProvider.System;
    
    [PhysicalRobotFact]
    void GivenMayWithTerminalPostureBehaviorController_WhenStartThenSleepThenStop_ThenSucceeds()
    {
        // Given
        var may = TestObjectMother.MaydayRobotFactory
            .CreateWithTerminalPostureBehaviorController(Terminal, TimeProvider)
            .RunUnsafe();

        // When
        Task.Run(may.Start);
        
        Thread.Sleep(TimeSpan.FromSeconds(0.5));

        may.Stop();

        // Then
        // Succeeds
    }
    
    [PhysicalRobotFact]
    void GivenMayWithBabyLegsBehaviorController_WhenStartThenSleepThenStop_ThenSucceeds()
    {
        // Given
        var may = TestObjectMother.MaydayRobotFactory
            .CreateWithBabyLegsBehaviorController(TimeProvider)
            .RunUnsafe();

        // When
        Task.Run(may.Start);
        
        Thread.Sleep(TimeSpan.FromSeconds(0.5));

        may.Stop();

        // Then
        // Succeeds
    }
}
