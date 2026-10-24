using Dynamixel;
using LanguageExt;
using MaydayDomain;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using Test.Unit;
using Test.Utilities;
using Xunit;
using Xunit.Abstractions;
using MaydayTestObjectMother = Test.Integration.MaydayDomain.Base.TestObjectMother;
using RotationDirection = RobotDomain.Structures.RotationDirection;

namespace Test.Integration.Dynamixel;

/// <summary>
/// Compares the rotation direction the Mayday model gives each joint with the direction stored in that servo's Drive
/// Mode register. Read-only: it opens the port and reads one register per servo, and never writes, enables torque or
/// initializes a servo, so it reports what the servos hold before <see cref="Driver.Initialize"/> changes it.
/// </summary>
public class RotationDirectionOnRobotTests(ITestOutputHelper testOutputHelper) : IDisposable
{
    readonly Lazy<NativeSerialPortCommunicationBus> _communicationBus =
        new(() => NativeSerialPortCommunicationBus.CreateInitialized().RunUnsafe());

    public static TheoryData<TestInput> DataFor_GivenRobot_WhenReadDriveMode_ThenDirectionBitMatchesModel()
    {
        TheoryData<TestInput> data = new();

        foreach (var legId in MaydayLegId.AllLegIds)
        foreach (var jointNumber in Enumerable.Range(1, MaydayLeg.JointCount))
            data.Add(new(Id: $"{legId.Side}{legId.SidePosition}-{jointNumber}", MaydayLegId: legId, JointNumber: jointNumber));

        return data;
    }

    [PhysicalRobotTheory]
    [MemberData(nameof(DataFor_GivenRobot_WhenReadDriveMode_ThenDirectionBitMatchesModel))]
    public void GivenRobot_WhenReadDriveMode_ThenDirectionBitMatchesModel(TestInput input)
    {
        // Given
        var jointId = input.MaydayLegId.JointId(input.JointNumber);
        var modelRotationDirection = ModelRotationDirectionOf(input.MaydayLegId, jointId);
        var directionBitExpected = global::Dynamixel.RotationDirection.FromDomain(modelRotationDirection).Value;

        // When
        var driveMode = _communicationBus.Value.Read(Id.FromBase(jointId), ControlRegister.DriveMode);

        // Then
        var directionBit = driveMode & DirectionBit;
        testOutputHelper.WriteLine(
            $"{input.Id.Value}: servo {jointId.Value}, Drive Mode {driveMode:b8}, model {modelRotationDirection}");

        Assert.True(
            directionBit == directionBitExpected,
            $"TestId: {input.Id.Value}, servo {jointId.Value}: Drive Mode {driveMode:b8} has direction bit {directionBit}, " +
            $"the model uses {modelRotationDirection} (bit {directionBitExpected}).");
    }

    // Drive Mode bit 0 is the direction: 0 normal, 1 reverse.
    const uint DirectionBit = 0b1;

    static RotationDirection ModelRotationDirectionOf(MaydayLegId legId, JointId jointId)
    {
        RecordingJointFactory jointFactory = new();
        new MaydayLegFactory(jointFactory, MaydayTestObjectMother.LegPostureByPositionMap).CreateLeg(legId);

        return jointFactory.RotationDirectionById[jointId];
    }

    public void Dispose()
    {
        if (_communicationBus.IsValueCreated)
            _communicationBus.Value.Dispose();
    }

    public record TestInput(TestId Id, MaydayLegId MaydayLegId, int JointNumber);

    /// <summary>Builds echo joints and records the rotation direction the leg factory asks for, by joint id.</summary>
    class RecordingJointFactory : JointFactory
    {
        readonly EchoJointFactory _echoJointFactory = new();

        public Dictionary<JointId, RotationDirection> RotationDirectionById { get; } = new();

        public Joint New(
            Link parent,
            Link child,
            Transform passiveTransform,
            JointId id,
            RotationDirection rotationDirection,
            AttachmentOrder attachmentOrder)
        {
            RotationDirectionById[id] = rotationDirection;
            return _echoJointFactory.New(parent, child, passiveTransform, id, rotationDirection, attachmentOrder);
        }
    }
}
