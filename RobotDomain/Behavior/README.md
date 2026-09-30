# Shared behavior contracts

## Responsibility

Define behavior start and terminal-driven command handling shared by robots.

## Public APIs

`BehaviorController`, `TerminalBehaviorController<TCommand>`.

## Key Concepts

Behavior selects planner goals; terminal input is an injected boundary.

## Dependencies

`Generic.System.Terminal` through `RobotDomain`.

## Dependency Rules

Do not reference Mayday/Ellie behavior implementations here.

## Invariants

Preserve cancellation and start lifecycle when implementing controllers.

## Architectural Constraints

Concrete terminal/hardware creation belongs at composition roots.

## Common Pitfalls

Avoid blocking or hidden global device access in shared behavior contracts.

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
