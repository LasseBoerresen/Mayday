# Physical tests leak the robot's serial adapter

## Symptoms

On 2026-10-24, with Mayday connected and clear to move, `dotnet test Test\Test.csproj --filter "Tier=Physical"` in one process passed only the first physical test that opened the adapter. Every later one failed with `Failed to open device with SN 'FT2H2YMW', status: FT_DEVICE_NOT_OPENED`: 9 failed, 1 passed, 2 skipped. Running only `MaydayRobotTests` gave the same pattern inside one class: the first test passed and the second failed.

## Investigation

Each physical test method run in a `dotnet test` process of its own passed or failed on its own merits:

- `NativeSerialPortCommunicationBusTests`, both `MaydayRobotTests` tests: passed.
- `MaydayLegTests.GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected`: failed on an assertion, not on the device (MAY-82).

So the device was free at the start of each process and taken after the first test in it.

## Root Cause

The FTDI device can be opened by one owner at a time, and the physical tests never release it. They create a `NativeSerialPortCommunicationBus`, either directly (`NativeSerialPortCommunicationBusTests` holds one in a field) or through `DynamixelJointFactory.Create` and `MaydayRobotFactory`, and never dispose it or the joint factory around it. `MaydayLegTests` keeps its joint factory in `static readonly` fields, so its port stays open for the whole test process. `NativeSerialPortCommunicationBus.Dispose` would close the port (`closePort`), but nothing calls it.

## Resolution

Worked around, not fixed: since [ADR 0011](../../docs/adr/0011-pre-push-gate-drives-the-robot.md), `gate.ps1 -Physical` runs each physical test method in its own process, so the operating system releases the port when each process ends. The fix, tests that dispose what they open, is tracked in [MAY-81](https://linear.app/mayday-robot/issue/MAY-81).

## Prevention

Give every physical test an owner for the bus or joint factory it creates (`using`, an `IDisposable` test class, or a fixture), never a `static` field. Until MAY-81 is done, run physical tests one method per process, as the gate does; a single `dotnet test --filter Tier=Physical` run reports failures that are not real.

## Related Components

[Dynamixel](../../Dynamixel/README.md), [Test](../../Test/README.md), [scripts](../../scripts/README.md)

## Related Tasks

[Physical pre-push gate](../task-summaries/physical-pre-push-gate.md), [MAY-81](https://linear.app/mayday-robot/issue/MAY-81)

## Related Decisions

[ADR 0011](../../docs/adr/0011-pre-push-gate-drives-the-robot.md), [ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
