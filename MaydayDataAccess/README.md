# MaydayDataAccess

## Responsibility

Persist and map Mayday leg posture-by-position data.

## Public APIs

`LegPostureByPositionMapFileRepo` (with `ForDefaultMapIn` for the map the robot uses, `DefaultLegPostureByPositionMap.json`), `MaydayLegPostureDao`, `Geometry.XyzDao`.

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

JSON object keys are strings; do not deserialize directly into complex coordinate keys. See [posture lookups that silently keep the current posture](../knowledge/pitfalls/posture-map-silent-fallback.md).

## Related ADRs

[Stable abstractions](../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

[Posture map accuracy](../knowledge/findings/posture-map-accuracy.md).

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
