# Mayday persistence geometry

## Responsibility

Convert domain coordinates to and from serialized JSON map keys.

## Public APIs

`XyzDao`.

## Key Concepts

JSON object keys must be strings, while domain positions retain units.

## Dependencies

`MaydayDataAccess` and `RobotDomain.Geometry`.

## Dependency Rules

Keep serialization-specific types outside generic geometry.

## Invariants

Coordinate key conversion must round-trip with stored maps.

## Architectural Constraints

Changing key format requires data migration/compatibility review.

## Common Pitfalls

Do not deserialize a JSON object directly into non-string coordinate keys.

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
