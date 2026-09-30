# RobotDomain

## Responsibility

Shared robotics vocabulary, structures, motion contracts, behavior contracts, geometry, and timing.

## Public APIs

See [Behavior](Behavior/README.md), [Geometry](Geometry/README.md), [Motion](Motion/README.md), [Physics](Physics/README.md), [Structures](Structures/README.md), and [Time](Time/README.md).

## Key Concepts

Joint/link graphs, units-aware state, timed goals, planners, drivers, periodic scheduling.

## Dependencies

`Generic` and external packages; no Mayday or Ellie project reference.

## Dependency Rules

Never depend on concrete robot projects, entry points, or Dynamixel native IO.

## Invariants

Physical dimensions use explicit units; hardware creation stays outside this layer.

## Architectural Constraints

[ADR 0001](../docs/adr/0001-stable-robot-abstractions.md) defines intended dependency direction.

## Common Pitfalls

Changing timing or interpolation without lagging-joint tests can change real motion.

## Related ADRs

[Stable abstractions](../docs/adr/0001-stable-robot-abstractions.md), [hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

[Current MaydayDomain dependency exception](../knowledge/findings/mayday-domain-hardware-reference.md).

## Related Root Causes

[Interpolation baseline drift](../knowledge/root-causes/interpolation-baseline-drift.md).

## Related Lessons Learned

[Goal versus measured state](../knowledge/lessons-learned/goal-versus-measured-state.md).

## Related Failed Attempts

[Measured-angle interpolation](../knowledge/failed-attempts/measured-angle-interpolation.md).
