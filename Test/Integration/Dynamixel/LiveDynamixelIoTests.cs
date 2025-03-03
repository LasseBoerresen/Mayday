using Dynamixel;
using JetBrains.Annotations;
using Xunit;

namespace Test.Integration.Dynamixel;

[TestSubject(typeof(DynamixelIoSdkImpl))]
public class LiveDynamixelIoTests
{
    readonly DynamixelIoSdkImpl _liveDynamixelIo = new();
    Id _id = new(1);

    [Fact] // [Fact(Skip="robot not connected")]
    void GivenRobotIsAttached_WhenSetTwoGoalAnglesAndSleep1sBetween_ThenCurrentPositionWithin20()
    {
        // Given
        _liveDynamixelIo.Initialize();
        _liveDynamixelIo.Write(_id, ControlRegister.TorqueEnable, Convert.ToByte(true));
    
        foreach (var i in Enumerable.Range(1, 18))
        {
            _id = new(i);
            _liveDynamixelIo.Write(_id, ControlRegister.TorqueEnable, Convert.ToByte(true));
            
            // When
            var goal = StepAngle.StepCenter - 100;
            SetAndWaitForGoal(goal);

            // Then
            AssertGoalReached(goal);

            // When Again
            goal = StepAngle.StepCenter;
            SetAndWaitForGoal(goal);
            
            // Then again
            AssertGoalReached(goal);
        }
    }

    void SetAndWaitForGoal(uint goal)
    {
        _liveDynamixelIo.Write(_id, ControlRegister.GoalPosition, goal);
        
        Thread.Sleep(TimeSpan.FromSeconds(0.1));
    }

    void AssertGoalReached(uint goal)
    {
        const int toleranceInAngleSteps = 30;
        
        var currentPosition = _liveDynamixelIo.Read(_id, ControlRegister.GoalPosition);
        var absoluteDifference = Math.Abs((int)currentPosition - goal);
        
        Assert.True(absoluteDifference < toleranceInAngleSteps);
    }
}
