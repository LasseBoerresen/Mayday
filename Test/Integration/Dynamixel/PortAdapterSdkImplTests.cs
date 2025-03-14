﻿using Dynamixel;
using JetBrains.Annotations;
using LanguageExt;
using Xunit;

namespace Test.Integration.Dynamixel;

[TestSubject(typeof(PortAdapterSdkImpl))]
public class PortAdapterSdkImplTests
{
    readonly PortAdapterSdkImpl _portAdapter = PortAdapterSdkImpl.CreateInitialized().RunUnsafe();
    Id _id = new(1);

    [Fact] // [Fact(Skip="robot not connected")]
    void GivenRobotIsAttached_WhenSetTwoGoalAnglesAndSleep1sBetween_ThenCurrentPositionWithin20()
    {
        // Given
        _portAdapter.Write(_id, ControlRegister.TorqueEnable, Convert.ToByte(true));
    
        foreach (var i in Enumerable.Range(1, 18))
        {
            _id = new(i);
            _portAdapter.Write(_id, ControlRegister.TorqueEnable, Convert.ToByte(true));
            
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
