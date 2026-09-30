# Robots

## Responsibility

Assemble Mayday structures, planners, behavior, and actuator factory into a runnable robot.

## Public APIs

`MaydayRobotFactory` and `Base.MaydayRobot` (`Start`, `Stop`).

## Key Concepts

Concrete composition, cancellation token ownership, alternative behavior configurations.

## Dependencies

`ManualBehavior` directly and its transitive domain/adapter/data-access dependencies.

## Dependency Rules

Compose concrete objects here rather than in the shared domain.

## Invariants

`Stop` cancels the owned token; creation can initialize physical hardware.

## Architectural Constraints

Treat factory methods returning `Eff<MaydayRobot>` as potentially hardware-affecting.

## Common Pitfalls

[Physical runs](../knowledge/pitfalls/physical-runs.md); do not call creation just to inspect configuration.

## Related ADRs

[Hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

[Current domain reference](../knowledge/findings/mayday-domain-hardware-reference.md).

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
