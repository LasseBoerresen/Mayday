using AwesomeAssertions;
using EllieMain;
using EllieMain.Behaviors;

namespace EllieMainTests.EndToEnd;

public class EllieTests
{
    [Fact]
    public void WhenCreateDefaultEllie__ThenIsNotNull()
    {
        // Given
        CancellationTokenSource cts = new();
        var behaviorController = new TerminalMovementBehaviorController(cts.Token);
        EllieFactory ellieFactory = new(behaviorController, cts);

        // When 
        var actualEllie = ellieFactory.CreateDefault(); 
        
        // Then
        actualEllie.Should().NotBeNull();
    }
}