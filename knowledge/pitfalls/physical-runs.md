# Pitfall

## Description

Running hardware-dependent tests or either robot executable as if they were hardware-free.

## Why It Is Dangerous

Startup can open device ports, enable or move actuators; uncontrolled conditions can damage a robot or environment.

## Warning Signs

Tests use `PhysicalRobotFact` or `PhysicalRobotTheory`, or code calls `NativeSerialPortCommunicationBus.CreateInitialized`, `DynamixelJointFactory.Create`, or robot `Start`. A safety setting that a later configuration source can override is a warning sign too: user secrets once overrode the gate's forced `MAYDAY_ROBOT_IS_CONNECTED=False`, and a gate run drove an attached robot ([root cause](../root-causes/user-secrets-overrode-gate-robot-flag.md)).

## Preferred Approach

Use echo joints, mocks, or hardware-independent tests first. Exclude physical tests by their `Tier=Physical` trait, as the gate does, rather than relying only on the robot flag. Confirm intended robot, port, environment, and operating conditions before an explicit physical run. See [CLAUDE.md](../../CLAUDE.md#hard-rules).

## Related Components

[Dynamixel](../../Dynamixel/README.md), [Test](../../Test/README.md), [Main](../../Main/README.md), [EllieMain](../../Ellie/EllieMain/README.md)
