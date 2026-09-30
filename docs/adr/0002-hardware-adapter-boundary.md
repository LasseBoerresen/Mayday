# Hardware access at the adapter boundary

## Status

Accepted

## Context

Robot behavior needs testable, replaceable actuator access. Device IO and periodic update timing have physical safety implications.

## Decision

Use `CommunicationBus` for Dynamixel IO, `JointFactory` and driver contracts at robot boundaries, and create concrete hardware in application/factory composition. Keep native interop in the adapter and serialize port access there. Validate with hardware-independent tests before physical runs.

## Alternatives Considered

- Call native SDK from robot behavior; ties planning to one device and complicates tests.
- Initialize hardware inside foundational structures; hides side effects and ownership.
- Supply contracts and compose concrete implementations at the outer edge; chosen approach.

## Why

`Dynamixel/CommunicationBus.cs` defines the IO interface; `NativeSerialPortCommunicationBus` locks the native port; `Robots/MaydayRobotFactory.cs` and `Ellie/EllieMain/Program.cs` construct concrete dependencies. `Test/Utilities/PhysicalRobotFactAttribute.cs` separates physical runs.

## Consequences

- Benefits: alternative implementations and hardware-independent tests.
- Costs: lifetime and cancellation must be coordinated at composition.
- Limitations: existing `MaydayDomain` reference to `Dynamixel` is documented as an exception, not endorsed as a new dependency rule.

## Related Components

[Dynamixel](../../Dynamixel/README.md), [Robots](../../Robots/README.md), [EllieMain](../../Ellie/EllieMain/README.md)

## Related ADRs

[0001](0001-stable-robot-abstractions.md)

## Related Findings

[MaydayDomain hardware reference](../../knowledge/findings/mayday-domain-hardware-reference.md)

## Related Root Causes

None recorded.
