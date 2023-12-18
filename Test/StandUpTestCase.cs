using mayday;
using mayday.Behavior;
using mayday.mayday;
using Test.Structure;

namespace Test;

public class StandUpTestCase
{
    [Fact]
    public void GivenFakeStructure_WhenStand_ThenStructurePostureIsThat()
    {
        // Given
        FakeMaydayStructure maydayStructure = new();
        StandBehaviorController behaviorController = new(maydayStructure);

        // When
        behaviorController.Stand();

        // Then
        var actual = maydayStructure.Posture;
        Assert.Equal(MaydayPosture.Standing, actual);
    }
}
