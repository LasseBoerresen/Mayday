# Mayday persistence geometry

## Overview

Converts tip positions between the domain's units-aware `Xyz` and the string
keys of the posture map's JSON file. It exists because JSON object keys must be
strings. Used only by `LegPostureByPositionMapFileRepo` in the parent project.

## Responsibility

Convert domain coordinates to and from serialized JSON map keys.

## Public APIs

None outside `MaydayDataAccess`: `XyzDao` is `internal`.

## Interface

Namespace `MaydayDataAccess.Geometry`.

### `XyzDao`

A position in meters, as plain numbers. `internal readonly record struct
XyzDao(double X, double Y, double Z)`. File: `XyzDao.cs`.

| Member | What it does |
|---|---|
| `FromDomain(Xyz)` | Takes each axis of the `Xyz` in meters. |
| `ToDomain()` | Builds an `Xyz` with `Length.FromMeters` on each axis. |
| `ToKey()` | Formats the key `"<X>,<Y>,<Z>"` with the invariant culture, for example `"-0.203125,-0.0625,0"`. |
| `FromKey(string)` | Splits the key on `,` and parses each part as a `double` with the invariant culture. Whitespace around a number is accepted: `" 0 , 0 , 0 "` is the origin. |

## Key Concepts

JSON object keys must be strings, while domain positions retain units.

## Dependencies

`MaydayDataAccess` and `RobotDomain.Geometry`.

## Dependency Rules

Keep serialization-specific types outside generic geometry.

## Invariants

Coordinate key conversion must round-trip with stored maps.

`FromKey` throws `FormatException` when the key does not split into exactly
three parts, or when a part is not a number in the invariant culture. `NaN`
and `Infinity` parse as numbers: `ToDomain` then throws `ArgumentException`
for `NaN` (from the `Xyz` constructor) and accepts an infinite coordinate.

## Side Effects

None: pure conversions, no I/O and no shared state.

## Architectural Constraints

Changing key format requires data migration/compatibility review.

## Common Pitfalls

Do not deserialize a JSON object directly into non-string coordinate keys.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDataAccess"`
  (hardware-free; `XyzDao` is covered through `LegPostureByPositionMapFileRepo.Load`).

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
