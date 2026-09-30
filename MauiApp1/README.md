# MauiApp1

## Responsibility

Experimental MAUI/Blazor user interface project.

## Public APIs

`MauiProgram.CreateMauiApp` and `MainPage`.

## Key Concepts

Multi-platform UI composition, separate from the Mayday and Ellie console entry points.

## Dependencies

`ManualBehavior` project reference and MAUI platform packages; see `MauiApp1.csproj`.

## Dependency Rules

Keep platform-specific UI concerns out of robot-domain libraries.

## Invariants

Building/running this project depends on platform workloads; it is not the primary robot startup.

## Architectural Constraints

Do not assume UI composition replaces `Main` or `EllieMain`.

## Common Pitfalls

Treating MAUI targets as ordinary net10 console tests.

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
