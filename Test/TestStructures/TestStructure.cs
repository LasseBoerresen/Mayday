using mayday;
using mayday.Structures;
using UnitsNet;

namespace Test.TestStructures;

public class TestStructure
{
    private readonly ICollection<Joint> _joints = new List<Joint>();
    private readonly Structure _structure;

    public TestStructure()
    {
        _structure = new(_joints);
    }

    [Fact]
    public void GivenNoComponents_WhenGetJointStates_ThenReturnsEmptyList()
    {
        // Given

        // When // Then
        AssertExpectedJoinStates(new JointState[]{});
    }

    [Fact]
    public void GivenOneJointWithZeroStateFakeMotor_WhenGetJointStates_ThenOneZeroState()
    {
        // Given
        _joints.Add(new(new ZeroStateFakeMotor()));

        // When // Then
        AssertExpectedJoinStates(new[] {JointState.Zero});
    }

    private void AssertExpectedJoinStates(IEnumerable<JointState> expected)
    {
        Assert.Equal(expected, _structure.JointStates);
    }
}
