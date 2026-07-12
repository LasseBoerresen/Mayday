using FluentAssertions;
using Robots;
using Xunit;

namespace Test.Unit.Robots.Mayday;

public class MaydayDayRobotFactoryTests
{
    [Fact]
    public void GivenMaydayRobotFactory_WhenCreateWithSwayBehavior__ThenIsNotNull()
    {
        // Given
        TimeProvider timeProvider = TimeProvider.System;
    
        // When
        var maydayRobotEff = MaydayRobotFactory.CreateWithSwayBehavior(timeProvider);

        // Then
        maydayRobotEff.Map(mr => mr.Should().NotBeNull());
    }
    
    // TODO Test that it actually has a the right behavior. By mocking
    //  BehaviorController and checking a method is called on it by the robot.  
}
