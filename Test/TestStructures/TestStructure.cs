using mayday;
using mayday.Structures;
using UnitsNet;

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

    [Fact]
    public void GivenOneJointWithZeroStateFakeMotor_WhenGetJointStates_ThenOneZeroStae()
    {
        // Given
        Joint[] joints = {new(new ZeroStateFakeMotor())};
        var structure = new Structure(joints);

        // When
        var actualJointStates = structure.JointStates;

        // Then
        JointState[] expextedJointStates = {JointState.Zero};
        Assert.Equal(expextedJointStates, actualJointStates);
    }
}
