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

`Q` runs in `float` precision (facade over `System.Numerics.Quaternion`): compare with `IsRotationAlmostEqual`, not exact equality.

## Architectural Constraints

Geometry changes propagate into pose estimates and motion limits.

## Common Pitfalls

Avoid unitless values or silently changing reference frames. See also the [Q `-` operator pitfall](../../knowledge/pitfalls/q-minus-operator-direction.md).

## Related ADRs

[Stable abstractions](../../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

[Q.Rotate rejected positions longer than 1.1 m](../../knowledge/root-causes/q-rotate-wrapped-position-in-q.md), [Transform.HalfWayTo returned the wrong transform](../../knowledge/root-causes/transform-halfway-to-wrong-result.md)

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
