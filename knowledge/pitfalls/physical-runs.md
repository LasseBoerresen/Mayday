# Pitfall

## Description

Running hardware-dependent tests or either robot executable as if they were hardware-free.

## Why It Is Dangerous

Startup can open device ports, enable or move actuators; uncontrolled conditions can damage a robot or environment.

## Warning Signs

Tests use `PhysicalRobotFact` or `PhysicalRobotTheory`, or code calls `NativeSerialPortCommunicationBus.CreateInitialized`, `DynamixelJointFactory.Create`, or robot `Start`.

## Preferred Approach

Use echo joints, mocks, or hardware-independent tests first. Confirm intended robot, port, environment, and operating conditions before an explicit physical run. See [CLAUDE.md](../../CLAUDE.md#hard-rules).

## Related Components

[Dynamixel](../../Dynamixel/README.md), [Test](../../Test/README.md), [Main](../../Main/README.md), [EllieMain](../../Ellie/EllieMain/README.md)
