# Dynamixel

## Responsibility

Adapt Dynamixel actuators and native serial communication to robot driver and joint contracts.

## Public APIs

`CommunicationBus`, `NativeSerialPortCommunicationBus`, `Driver`, `DynamixelJointFactory`, register/unit conversion types.

## Key Concepts

IDs, control registers, batched reads/writes, motor angle and speed conversion.

## Class relationships

The factory creates a joint driver backed by a Dynamixel driver and communication bus. The bus interface keeps native port access replaceable; arrows below denote implementation or use, not project references.

```mermaid
classDiagram
    class JointFactory
    class JointDriver
    class ActuatorDriver
    class CommunicationBus
    class DynamixelJointFactory
    class PeriodicallyBatchedJointDriver
    class Driver
    class NativeSerialPortCommunicationBus
    DynamixelJointFactory ..|> JointFactory
    DynamixelJointFactory --> JointDriver : creates joints with
    PeriodicallyBatchedJointDriver ..|> JointDriver
    PeriodicallyBatchedJointDriver --> ActuatorDriver : uses
    Driver ..|> ActuatorDriver
    Driver --> CommunicationBus : uses
    NativeSerialPortCommunicationBus ..|> CommunicationBus
```

## Dependencies

`Generic`, `RobotDomain`, and native/vendor packages in `Dynamixel.csproj`.

## Dependency Rules

Keep native SDK calls within this adapter; higher-level code should use stable contracts.

## Invariants

`NativeSerialPortCommunicationBus` serializes operations on its port; preserve lock coverage for shared native state.

`StepAngle` maps XL430 position steps 0 to 4,095 (4,096 counts per revolution, 0.088 degrees per count, center step 2,048) to angles from -0.5 up to, but not including, 0.5 revolutions, and rounds an angle to the nearest step. Every step the actuator can report round-trips through `ToAngle` and `ToSteps`, including both ends of the range. Source: Robotis e-Manual, XL430-W250 control table.

## Architectural Constraints

Opening ports and enabling/moving motors are explicit hardware actions; do not run casually.

## Common Pitfalls

[Physical runs](../knowledge/pitfalls/physical-runs.md); uncoordinated port access can corrupt communication.

## Related ADRs

[Hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

[MaydayDomain reference](../knowledge/findings/mayday-domain-hardware-reference.md).

## Related Root Causes

[Interpolation baseline drift](../knowledge/root-causes/interpolation-baseline-drift.md).

## Related Lessons Learned

[Goal versus measured state](../knowledge/lessons-learned/goal-versus-measured-state.md).

## Related Failed Attempts

[Measured-angle interpolation](../knowledge/failed-attempts/measured-angle-interpolation.md).
