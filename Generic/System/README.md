# System abstractions

## Overview

Provide terminal IO behind a replaceable interface, so consumers can substitute
a test terminal for console input/output.

## Responsibility

Provide terminal IO behind a replaceable interface.

## Interface

- `Generic.System.Terminal` (`Terminal.cs`): interface with
  `Write(string? value)`, `WriteLine(string? value)` and `string? ReadLine()`.
- `Generic.System.SystemTerminal` (`SystemTerminal.cs`): `Terminal` backed by
  `System.Console` (`Console.Write`, `Console.WriteLine`, `Console.ReadLine`).
- Known other implementation: `Test.Utilities.TestTerminal`
  ([Test/Utilities/TestTerminal.cs](../../Test/Utilities/TestTerminal.cs)), a
  test double fed from a `Queue<string>` of input lines.

## Key Concepts

Consumers can substitute a test terminal for console input/output.

## Dependencies

Part of `Generic`; no robot-specific project dependency.

## Dependency Rules

Do not expose device-specific robotics operations here.

## Invariants

- `Terminal.ReadLine` may return null; callers handle missing input
  (`string? ReadLine()` in `Terminal.cs`).
- `Write` and `WriteLine` accept null values (`string? value`).
- `SystemTerminal` holds no state; every call goes straight to `Console`.

## Side Effects

- `SystemTerminal.Write` / `WriteLine` write to the process's standard output.
- `SystemTerminal.ReadLine` reads standard input and blocks until a line or
  end of input arrives.
- `Terminal` itself has no side effects; they depend on the implementation.

## Architectural Constraints

Keep system IO at the boundary, not inside reusable robot models.

## Common Pitfalls

Do not assume console IO is available in headless tests.

## Verification

- Build: `dotnet build Generic\Generic.csproj`
- No tests target this namespace directly; `TestTerminal` exercises the
  `Terminal` contract in the `Test` project: `dotnet test Test\Test.csproj`

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
