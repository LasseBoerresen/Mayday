# Generic

## Responsibility

Shared utility and system-facing abstractions independent of a particular robot.

## Public APIs

`Generic.System.Terminal`, `SystemTerminal`, and extension methods for units, time, dictionaries, and sequences.

## Key Concepts

Small reusable transformations and a substitutable terminal boundary.

## Dependencies

No project references; inspect `Generic.csproj` for package dependencies.

## Dependency Rules

Do not add Mayday, Ellie, or hardware project references here.

## Invariants

Utilities should not create robot hardware or own robot lifecycle.

## Architectural Constraints

Prefer explicit dependencies over global system access in domain code.

## Common Pitfalls

Do not put robot-specific behavior in generic extension methods.

## Related ADRs

[Stable abstractions](../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
