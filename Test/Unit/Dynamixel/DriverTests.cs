using Dynamixel;
using JetBrains.Annotations;
using Moq;
using Xunit;
using RotationDirection = RobotDomain.Structures.RotationDirection;

namespace Test.Unit.Dynamixel;

[TestSubject(typeof(Driver))]
public class DriverTests
{
    readonly Mock<CommunicationBus> _communicationBusMock = new();
    readonly Id _id = new(1);

    public DriverTests()
    {
        _communicationBusMock.Setup(cb => cb.Ping(It.IsAny<Id>())).Returns(true);
    }

    public static TheoryData<TestInput> DataFor_GivenDriveMode_WhenInitializeWithRotationDirection_ThenWritesExpectedDriveMode()
    {
        // Drive Mode bit 0 is the direction (0 normal, 1 reverse). Bit 2 is the time-based profile and bit 3 torque on
        // by goal update; whatever the servo has stored there, the driver turns them off.
        return new()
        {
            new(Id: "0", DriveMode: 0b0000, RotationDirection: RotationDirection.Reverse, DriveModeExpected: 0b0001),
            new(Id: "1", DriveMode: 0b0001, RotationDirection: RotationDirection.Reverse, DriveModeExpected: 0b0001),
            new(Id: "2", DriveMode: 0b0100, RotationDirection: RotationDirection.Reverse, DriveModeExpected: 0b0001),
            new(Id: "3", DriveMode: 0b1101, RotationDirection: RotationDirection.Reverse, DriveModeExpected: 0b0001),
            new(Id: "4", DriveMode: 0b0000, RotationDirection: RotationDirection.Forward, DriveModeExpected: 0b0000),
            new(Id: "5", DriveMode: 0b0001, RotationDirection: RotationDirection.Forward, DriveModeExpected: 0b0000),
            new(Id: "6", DriveMode: 0b1101, RotationDirection: RotationDirection.Forward, DriveModeExpected: 0b0000),
        };
    }

    [Theory]
    [MemberData(nameof(DataFor_GivenDriveMode_WhenInitializeWithRotationDirection_ThenWritesExpectedDriveMode))]
    public void GivenDriveMode_WhenInitializeWithRotationDirection_ThenWritesExpectedDriveMode(TestInput input)
    {
        // Given
        _communicationBusMock
            .Setup(cb => cb.Read(_id, ControlRegister.DriveMode))
            .Returns(input.DriveMode);

        Driver driver = new(_communicationBusMock.Object);

        // When
        driver.Initialize(_id, input.RotationDirection);

        // Then
        _communicationBusMock.Verify(
            cb => cb.Write(_id, ControlRegister.DriveMode, input.DriveModeExpected),
            Times.Once,
            $"TestId: {input.Id}");
    }

    public record TestInput(
        TestId Id,
        uint DriveMode,
        RotationDirection RotationDirection,
        uint DriveModeExpected);
}
