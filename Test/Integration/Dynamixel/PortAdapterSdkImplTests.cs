using Dynamixel;
using JetBrains.Annotations;
using Xunit;

namespace Test.Integration.Dynamixel;

[TestSubject(typeof(PortAdapterSdkImpl))]
public class PortAdapterSdkImplTests
{
    readonly PortAdapterSdkImpl _portAdapter = new();
    readonly Id _id = new(1);

    [Fact]
    void GivenRobotIsAttachedAndId1_WhenSetTwoGoalAnglesAndSleep1sBetween_ThenCurrentPositionWithin20()
    {
        // Given
        _portAdapter.Initialize();
        _portAdapter.Write(_id, ControlRegister.TorqueEnable, Convert.ToByte(true));

        // When
        var goal = PositionAngle.StepCenter + 300;
        SetAndWaitForGoal(goal);

        // Then
        AssertGoalReached(goal);

        // When Again
        goal = PositionAngle.StepCenter;
        SetAndWaitForGoal(goal);
        
        // Then again
        AssertGoalReached(goal);
    }

    void SetAndWaitForGoal(uint goal)
    {
        _portAdapter.Write(_id, ControlRegister.GoalPosition, goal);
        Thread.Sleep(TimeSpan.FromSeconds(1));
    }

    void AssertGoalReached(uint goal)
    {
        const int toleranceInAngleSteps = 20;
        
        var currentPosition = _portAdapter.Read(_id, ControlRegister.GoalPosition);
        var absoluteDifference = Math.Abs((int)currentPosition - goal);
        
        Assert.True(absoluteDifference < toleranceInAngleSteps);
    }
}
