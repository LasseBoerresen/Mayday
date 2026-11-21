# Unit tests of Generic

## Overview

Unit tests of the shared utility project [Generic](../../../Generic/README.md)
(namespace `Test.Unit.Generic`). Today they cover only
`UnitsNetExtensions.IsAlmostEqualSingleRotation`.

## Interface

### `UnitsNetExtensionsTests`

Checks when two angles count as almost equal modulo full turns.
File: `UnitsNetExtensionsTests.cs`.

| Test | What it checks |
|---|---|
| `GivenAngleAndOther_WhenIsAlmostEqualSingleRotation_ThenReturnsExpected` | Theory with `[InlineData]`: with a precision of 0.1 revolutions, 0 rev is almost equal to 0, 0.01, 1.0, 0.99, -0.99 and 1.99 rev, and not to 0.11 or 1.89 rev. |

The test writes each row's `testId` to xUnit's `ITestOutputHelper`. Two rows
share the `testId` `"6"` (1.99 and 1.89 rev).

## Invariants

- The expectations treat angles a whole number of turns apart as equal, so a
  change to `IsAlmostEqualSingleRotation` that compares raw angle values fails
  the rows for 1.0, 0.99, -0.99 and 1.99 rev.

## Side Effects

- None besides xUnit test output.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.Generic"`.
  No `Tier=Physical` or `Tier=Manual` tests are included.
