# Pitfall

## Description

Running hardware-dependent tests or either robot executable as if they were hardware-free.

## Why It Is Dangerous

Startup can open device ports, enable or move actuators; uncontrolled conditions can damage a robot or environment.

## Warning Signs

Tests use `PhysicalRobotFact` or `PhysicalRobotTheory`, or code calls `NativeSerialPortCommunicationBus.CreateInitialized`, `DynamixelJointFactory.Create`, or robot `Start`.

A physical test that was expected to skip reports passes and failures instead of skips. On 2026-10-24 a new read-only `[PhysicalRobotTheory]` was run with `dotnet test --filter` to check that it skips; the Test user secrets said the robot was connected, so it opened the port and read all 18 servos without the user's confirmation. It only read registers, so nothing moved.

## Preferred Approach

Use echo joints, mocks, or hardware-independent tests first. Before any `dotnet test` whose filter can match a physical test, read the flags (`dotnet user-secrets list --project Test/Test.csproj`, without changing them) or exclude `Tier=Physical` and `Tier=Manual`; to check that a new physical test skips, use `scripts\gate.ps1`, which forces the robot to not clear-to-move. Confirm intended robot, port, environment, and operating conditions before an explicit physical run. See [CLAUDE.md](../../CLAUDE.md#hard-rules). A `git push` of code is a physical run: the pre-push gate drives the robot when the operator's `MAYDAY_ROBOT_IS_CLEAR_TO_MOVE` is `True` ([ADR 0011](../../docs/adr/0011-pre-push-gate-drives-the-robot.md)).

## Related Components

[Dynamixel](../../Dynamixel/README.md), [Test](../../Test/README.md), [Main](../../Main/README.md), [EllieMain](../../Ellie/EllieMain/README.md)
