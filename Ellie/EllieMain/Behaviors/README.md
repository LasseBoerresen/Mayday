# Ellie behaviors

## Responsibility

Interpret terminal movement commands as Ellie planner requests.

## Public APIs

`TerminalMovementBehaviorController`, `MovementCommand`.

## Key Concepts

Manual command selection and cancellation.

## Dependencies

`EllieMain.MotionPlanning`, `Generic.System.Terminal`, shared behavior contracts.

## Dependency Rules

Do not perform native bus operations inside command handling.

## Invariants

Preserve command meaning and cancellation when extending behavior.

## Architectural Constraints

Planner/structure motion paths are not fully implemented.

## Common Pitfalls

Do not claim a terminal command causes safe physical movement until its planner path exists and is tested.

## Related ADRs

[Hardware boundary](../../../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
