# Shared geometry

## Responsibility

Provide coordinate, rotation, transform, and intersection abstractions.

## Public APIs

`Xyz`, `Q`, `Rpy`, `Transform`, `Ray3D`, `Triangle3D`, and `SystemNumerics` helpers.

## Key Concepts

Units-aware translations, orientations, and transform composition.

## Dependencies

Part of `RobotDomain`, with units and numerics packages.

## Dependency Rules

Keep Mayday/Ellie-specific measurements in their respective robot projects.

## Invariants

Preserve coordinate and angle conventions for composition and kinematics.

- `Q` is a rotation quaternion: its components are validated to be plausible for a rotation (|v| <= 1.1), so never wrap a position in one. `Q.Rotate` rotates an `Xyz` as a vector.
- `Xyz` is any spatial coordinate and has no magnitude limit; only NaN is rejected.
- `Q` is a facade over `System.Numerics.Quaternion` for non-hot paths. Its operations run in `float` precision, so compare results with `IsRotationAlmostEqual`/`IsAlmostEqual`, not exact equality.

## Architectural Constraints

Geometry changes propagate into pose estimates and motion limits.

## Common Pitfalls

Avoid unitless values or silently changing reference frames.

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
