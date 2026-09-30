# Ellie

## Responsibility

Group Ellie's independent robot application and its end-to-end tests.

## Public APIs

See [EllieMain](EllieMain/README.md) and [EllieMainTests](EllieMainTests/README.md).

## Key Concepts

Articulated steering and wheel-driven motion, separate from Mayday's legged structure.

## Dependencies

`EllieMain` references `RobotDomain` and `Dynamixel`; test dependencies are described in `EllieMainTests`.

## Dependency Rules

Share foundational contracts, not Mayday-specific structure.

## Invariants

Some Ellie motion/structure operations remain unimplemented.

## Architectural Constraints

Keep concrete hardware startup in Ellie's application composition.

## Common Pitfalls

[Physical runs](../knowledge/pitfalls/physical-runs.md).

## Related ADRs

[Stable abstractions](../docs/adr/0001-stable-robot-abstractions.md), [hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
