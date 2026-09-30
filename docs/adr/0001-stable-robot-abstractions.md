# Stable robot abstractions

## Status

Accepted

## Context

Mayday and Ellie share robot concepts but have different structures, behavior, and assembly. The [root architecture description](../../README.md) calls for dependencies on more stable modules.

## Decision

Keep shared geometry, structures, motion contracts, behavior contracts, and timing in `RobotDomain`; keep robot-specific structure and planning in the respective robot projects. Application/composition code depends on these contracts, not the reverse.

## Alternatives Considered

- Put all robot models and orchestration in one shared project; this would couple both robots.
- Move device-specific constructs into shared contracts; this would make simulation and replacement harder.
- Keep robot-specific logic in its own project while sharing stable concepts; chosen approach.

## Why

The root README's dependency-inversion goal and the two distinct application compositions favor stable, reusable contracts.

## Consequences

- Benefits: independently replaceable robots and adapters.
- Costs: explicit composition and contracts at boundaries.
- Limitations: the existing `MaydayDomain` to `Dynamixel` project reference does not yet follow this direction; do not remove it without tracing usage and tests.

## Related Components

[RobotDomain](../../RobotDomain/README.md), [MaydayDomain](../../MaydayDomain/README.md), [EllieMain](../../Ellie/EllieMain/README.md)

## Related ADRs

[0002](0002-hardware-adapter-boundary.md)

## Related Findings

[MaydayDomain hardware reference](../../knowledge/findings/mayday-domain-hardware-reference.md)

## Related Root Causes

None recorded.
