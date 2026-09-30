# Mayday components

## Responsibility

Encode hexapod link dimensions and mounting geometry.

## Public APIs

`Coxa`, `Femur`, `Tibia`, `Thorax`.

## Key Concepts

Physical leg sections used by Mayday structure and kinematics.

## Dependencies

Part of `MaydayDomain`; shared geometry from `RobotDomain`.

## Dependency Rules

Do not move Mayday-specific dimensions into generic robot geometry.

## Invariants

Mount translations and orientations must agree with `MaydayLegFactory`.

## Architectural Constraints

Geometry changes affect reachable postures and physical safety.

## Common Pitfalls

Do not change a measurement without checking kinematics and joint limits.

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
