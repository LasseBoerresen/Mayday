using Dynamixel;
using LanguageExt;
using MaydayDomain;
using RobotDomain.Geometry;
using RobotDomain.Time;
using Test.Utilities;
using UnitsNet;
using Xunit;
using Length = UnitsNet.Length;
using MaydayTestObjectMother = Test.Integration.MaydayDomain.Base.TestObjectMother;

namespace Test.Integration.MaydayDomain;

/// <summary>
/// Moves the real Mayday and asks the person watching whether it moved as expected.
/// </summary>
public class MaydayStructureManualTests
{
    static readonly TimeProvider TimeProvider = TimeProvider.System;
    static readonly TimeSpan MoveDuration = TimeSpan.FromSeconds(2);
    static readonly TimeSpan SettleTime = TimeSpan.FromSeconds(1);
    static readonly HumanVerifier Human = new(new ConsoleWindowHumanPrompt(timeout: TimeSpan.FromMinutes(10)));

    [ManualRobotFact]
    public void GivenSittingTallRobot_WhenMoveAllTipsForward5cm_ThenAllTipsMoveForward()
    {
        // Given
        using CancellationTokenSource cts = new();
        using var jointFactory = DynamixelJointFactory
            .Create(cts, TimeProvider, new FailFastFatalErrorHandler())
            .RunUnsafe();
        var structure = new MaydayStructureFactory(
                new MaydayLegFactory(jointFactory, MaydayTestObjectMother.DefaultLegPostureByPositionMap))
            .CreateDefault();

        // From Sitting, the posture map has no posture for the rear legs' forward targets, which pull their tips in
        // towards the body, so they would silently keep their posture. From SittingTall all six targets are mapped.
        MoveAndSettle(() => structure.SetPostureForAllLegs(
            TimeProvider.ScheduleIn(MaydayLegPosture.SittingTall, MoveDuration)));

        Assert.True(
            Human.Confirms("Is Mayday sitting tall, with all six legs pointing straight out from the body?"),
            "Precondition: Mayday did not sit tall with straight legs.");

        // When
        var forward5Cm = Xyz.Zero with { X = Length.FromCentimeters(5) };
        MoveAndSettle(() => structure.MoveTipsBy(TimeProvider.ScheduleIn(forward5Cm, MoveDuration)));

        // Then
        var allTipsMovedForward = Human.Confirms(
            "Did all six leg tips move about 5 cm forward, towards Mayday's front?");

        MoveAndSettle(() => structure.SetPostureForAllLegs(
            TimeProvider.ScheduleIn(MaydayLegPosture.Sitting, MoveDuration)));
        cts.Cancel();

        Assert.True(allTipsMovedForward, "The person watching saw a leg tip that did not move forward.");
    }

    static void MoveAndSettle(Action move)
    {
        move();
        Thread.Sleep(MoveDuration + SettleTime);
    }
}
