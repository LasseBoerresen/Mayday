# Shared physics values

## Responsibility

Represent physical quantities not directly modeled by generic numeric types.

## Public APIs

`LoadRatio`.

## Key Concepts

Unit-aware actuator load representation.

## Dependencies

Part of `RobotDomain`; no robot-specific project references.

## Dependency Rules

Keep vendor register conversion in the adapter, not here.

## Invariants

Preserve value semantics and physical meaning of load ratios.

## Architectural Constraints

Do not substitute unitless values for domain quantities.

## Common Pitfalls

Do not conflate an actuator's raw register value with a domain load ratio.

## Related ADRs

[Stable abstractions](../../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
