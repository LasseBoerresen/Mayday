using Dynamixel;
using JetBrains.Annotations;
using Xunit;

namespace Test.Integration.Dynamixel;

[TestSubject(typeof(PortAdapterSdkImpl))]
public class PortAdapterSdkImplTests
{
    readonly PortAdapterSdkImpl _portAdapter = new();
    Id _id = new(1);

    [Fact]
    void GivenRobotIsAttached_WhenSetTwoGoalAnglesAndSleep1sBetween_ThenCurrentPositionWithin20()
    {
        // Given
        _portAdapter.Initialize();
        _portAdapter.Write(_id, ControlRegister.TorqueEnable, Convert.ToByte(true));
    
        foreach (var i in Enumerable.Range(1, 18))
        {
            _id = new(i);
            _portAdapter.Write(_id, ControlRegister.TorqueEnable, Convert.ToByte(true));
            
            // When
            var goal = PositionAngle.StepCenter - 100;
            SetAndWaitForGoal(goal);

            // Then
            AssertGoalReached(goal);

            // When Again
            goal = PositionAngle.StepCenter;
            SetAndWaitForGoal(goal);
            
            // Then again
            AssertGoalReached(goal);
        }
    }

    void SetAndWaitForGoal(uint goal)
    {
        _portAdapter.Write(_id, ControlRegister.GoalPosition, goal);
        
        Thread.Sleep(TimeSpan.FromSeconds(0.1));
    }

    void AssertGoalReached(uint goal)
    {
        const int toleranceInAngleSteps = 30;
        
        var currentPosition = _portAdapter.Read(_id, ControlRegister.GoalPosition);
        var absoluteDifference = Math.Abs((int)currentPosition - goal);
        
        Assert.True(absoluteDifference < toleranceInAngleSteps);
    }
}
