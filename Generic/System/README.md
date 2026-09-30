# System abstractions

## Responsibility

Provide terminal IO behind a replaceable interface.

## Public APIs

`Terminal` and `SystemTerminal`.

## Key Concepts

Consumers can substitute a test terminal for console input/output.

## Dependencies

Part of `Generic`; no robot-specific project dependency.

## Dependency Rules

Do not expose device-specific robotics operations here.

## Invariants

`Terminal.ReadLine` may return null; callers handle missing input.

## Architectural Constraints

Keep system IO at the boundary, not inside reusable robot models.

## Common Pitfalls

Do not assume console IO is available in headless tests.

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
