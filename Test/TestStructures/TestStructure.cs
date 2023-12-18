using mayday;
using mayday.Structures;

namespace Test.TestStructures;

public class TestStructure
{
    [Fact]
    public void GivenNoComponents_WhenGetJointStates_ThenReturnsEmptyList()
    {
        // Given
        Joint[] joints = {};
        var structure = new Structure(joints);

        // When
        var actualJointStates = structure.JointStates;

        // Then
        JointState[] expextedJointStates = {};
        Assert.Equal(expextedJointStates, actualJointStates);
    }
}
