# Dynamixel adapter

This project is the hardware boundary ([ADR 0002](../docs/adr/0002-hardware-adapter-boundary.md)). Read [README.md](README.md) for the class relationships.

## Boundary

- Native and vendor calls stay inside this project, behind `CommunicationBus` (`LowLevelCommunicationBusNativeInterop` → `NativeSerialPortCommunicationBus`). Higher layers see only `RobotDomain` joint and driver contracts.
- Convert raw register values to `UnitsNet` quantities here (`StepAngle`, `StepSpeed`, `StepTemperature`). Validate IDs, ports and register values at this boundary and throw clearly on invalid configuration.
- Torque enable, motion, device initialization and port opening happen only in explicitly named operations (`Driver.Initialize`, `NativeSerialPortCommunicationBus.CreateInitialized`) reached from composition; keep constructors and queries free of hardware side effects.
- Register addresses come from `XL430_W250_control_table.csv`; check it before adding or changing a register.

## Threading and timing

- `NativeSerialPortCommunicationBus` serializes every port operation with a lock, because the native library shares communication state across calls. Route all port access through it; make access concurrent only after establishing that the native library and protocol support it.
- The joint driver (`RobotDomain.Motion.PeriodicallyBatchedJointDriver`, driving this adapter) runs faster than behavior and planning, which hand it immutable goals. Each periodic component owns its mutable state; keep lock scopes minimal.
- Bus round-trips are the control-loop bottleneck (see [DevLog](../DevLog.md)). Measure before and after any performance change, and preserve scheduling and communication correctness.
- Document thread-safety assumptions on the affected API.

## Verifying changes

Use `Test/Unit/Dynamixel` and `EchoJointDriver` for hardware-free checks. Code here can move the robot: see [the physical-runs pitfall](../knowledge/pitfalls/physical-runs.md) before any physical test.
