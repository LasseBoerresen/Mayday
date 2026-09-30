# MaydayDataAccess

## Responsibility

Persist and map Mayday leg posture-by-position data.

## Public APIs

`LegPostureByPositionMapFileRepo`, `MaydayLegPostureDao`, `Geometry.XyzDao`.

## Key Concepts

JSON string keys converted to units-aware position keys and frozen lookup maps.

## Dependencies

`MaydayDomain`, `RobotDomain`; see `MaydayDataAccess.csproj`.

## Dependency Rules

File formats and DAO conversion stay in persistence, not robot structure abstractions.

## Invariants

Loading requires a readable, valid map; storing orders coordinates for stable output.

## Architectural Constraints

The application supplies the file path and owns file availability.

## Common Pitfalls

JSON object keys are strings; do not deserialize directly into complex coordinate keys.

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
