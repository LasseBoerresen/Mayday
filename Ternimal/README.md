# Ternimal

## Responsibility

Standalone experimental console project (directory spelling retained).

## Public APIs

`Program.cs` currently writes a greeting; no reusable API.

## Key Concepts

Separate from `Generic.System.Terminal`, which is the shared terminal contract.

## Dependencies

No project references in `Ternimal.csproj`; targets net8.0.

## Dependency Rules

Do not route primary robot terminal IO through this project without a decision.

## Invariants

No production robot behavior is currently implemented here.

## Architectural Constraints

Keep experiments independent of robot startup.

## Common Pitfalls

Confusing this executable with the `Generic.System` terminal abstraction.

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
