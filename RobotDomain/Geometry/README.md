# Shared geometry

## Responsibility

Provide coordinate, rotation, transform, and intersection abstractions.

## Interface

- **Positions:** `Xyz(Length X, Length Y, Length Z)`, or `double`s in meters (`Xyz.cs`). Vector arithmetic (`+`, `-`, unary `-`, `* double`), `Length`, distances to a point or line segment, `GetFractionOfProgressBetween(start, end)` as a `Ratio`, and `Sum()`/`Mean()` over a sequence (`XyzExtensions`). `AsVector3Meters()`/`FromVector3Meters(Vector3)` convert to and from `System.Numerics`.
- **Rotations:** `Q(double W, double X, double Y, double Z)`, a rotation quaternion (`Q.cs`), built from `Unit`, `FromRpy(Rpy)` or `FromAxisAngle(Vector3, Angle)`. `Rotate(Xyz)` rotates a position; `Angle`, `Axis`, `Inverse(Q)`, `Dot(Q)` and `* double` (scales the angle about the same axis). `Rpy(Angle R, Angle P, Angle Y)`, or `double`s in revolutions, gives Tait-Bryan angles (`Rpy.cs`).
- **Poses:** `Transform(Xyz Xyz, Q Q)`, a position plus a rotation (`Transform.cs`), built from `Zero`, `FromXyz` or `FromQ`. Composed with `+`/`Add` and `-`/`Subtract` (see Key Concepts); `InDirectionTo(other, factor)` and `HalfWayTo(other)` interpolate between two poses.
- **Velocities:** `Twist(XyzSpeed Xyz, RpySpeed Rpy)`, built from `XyzSpeed(Speed X, Speed Y, Speed Z)` and `RpySpeed(RotationalSpeed R, P, Y)` (`Twist.cs`, `XyzSpeed.cs`, `RpySpeed.cs`).
- **Intersections:** `Ray3D(Xyz Origin, Xyz Direction)` with `DistanceToPlane(Plane)` (`Ray3D.cs`). `Triangle3D(Xyz v0, Xyz v1, Xyz v2)` with `Area()`, `Center()`, `IsColinear()`, `ToPlane()` and `LookForIntersectionWith(Ray3D)`, which returns a `Ray3DIntersection(Xyz Point, Length Distance)` or `null` (`Triangle3D.cs`, `Ray3DIntersection.cs`).
- **Comparison:** `Xyz.IsAlmostEqual(Xyz, Length)` and `Rpy.IsAlmostEqual(Rpy, Angle)` compare per component. `Q.IsOrientationAlmostEqual` treats q and −q as the same orientation, and is the one to use for orientations. `Q.IsRotationAlmostEqual` is sign-sensitive. Both `Q` methods take a dot-product tolerance (default 1e-5), not an angle. `Transform.IsAlmostEqual(other, Length, Angle)` compares position and orientation.
- **Test data:** `Random(…)` on `Xyz`, `Q`, `Rpy` and `Transform`.
- [SystemNumerics](SystemNumerics/README.md): `float` `System.Numerics` ray, triangle and `Vector3` helpers behind `Ray3D` and `Triangle3D`.

## Key Concepts

Units-aware translations, orientations, and transform composition.

- **Units:** lengths are UnitsNet `Length`, angles `Angle`. The `double` constructors take meters (`Xyz`) and revolutions (`Rpy`).
- **Frames:** right-handed coordinates with right-handed rotations.
- **`Rpy` convention:** yaw-pitch-roll order about z, y and x; intrinsic (the axes move with each rotation); active (the point rotates, not the frame).
- **Composition:** `a + b` gives `b` expressed in the frame of `a`. For a `Transform` that is `(a.Xyz + a.Q.Rotate(b.Xyz), a.Q + b.Q)`; for a `Q` it is the product `a·b`.
- **Difference:** `Transform` `a - b` is `a` relative to `b`, so `b + (a - b) = a`. `Q`'s `a - b` is `inverse(a)·b`, which is `b` relative to `a`: the opposite direction. See the [Q `-` operator pitfall](../../knowledge/pitfalls/q-minus-operator-direction.md).

## Dependencies

Part of `RobotDomain`. Uses:
- `UnitsNet` for every physical quantity;
- `System.Numerics` (`Vector3`, `Quaternion`, `Plane`) for the math underneath;
- `Generic`, for unit-aware comparison (`UnitsNetExtensions`) and random helpers;
- its own [SystemNumerics](SystemNumerics/README.md) folder.

`Xyz.cs` imports `LanguageExt` but uses none of it.

## Dependency Rules

Keep robot-specific measurements in the robot projects that use this one.

## Invariants

Preserve coordinate and angle conventions for composition and kinematics.

- **Precision:** `Q` and `Xyz` store `double`. `Q.FromRpy`, `Q`'s normalization and `Xyz.Length` compute in `double`, but most rotation and vector math runs in `float` through `System.Numerics` (`Q`'s operators, `Rotate`, `Inverse`, `Dot`; `Xyz` distances and the closest point on a segment; `Ray3D`; `Triangle3D`). Compare results with a tolerance, never with exact equality.
- **Validation:** `Xyz`, `Q` and `Rpy` throw `ArgumentException` on NaN components; `Q` also throws when any component exceeds 1.1 in magnitude. Values made with `with` or `init` skip this validation. `Q` is not normalized on construction; `Rotate` normalizes before use.
- **Wrapping:** `Rpy` wraps each angle to a single rotation (`revolutions % 1.0`, sign kept), so `Rpy.One` equals `Rpy.Zero`.
- `Q.Angle` is `2·acos(W)`; `Q.Axis` is `UnitZ` for the identity.
- `XyzExtensions.Mean` throws `ArgumentException` for an empty sequence; `Triangle3D.FromList` throws `ArgumentException` unless given exactly 3 vertices.
- `Ray3D.DistanceToPlane` does not guard against a parallel ray or a plane behind the origin. It and `Triangle3D.LookForIntersectionWith` return the ray parameter `t`, which is a distance in meters only for a unit-length `Direction`.
- All types are immutable records or readonly record structs.

## Known Issues

- `Transform.IsAlmostEqual` passes the angle precision, in revolutions, as the dot-product tolerance of `Q.IsOrientationAlmostEqual`, so it accepts far larger rotation differences than asked: [MAY-91: Need: Transform.IsAlmostEqual compares rotations within the given angle](https://linear.app/mayday-robot/issue/MAY-91).
- The 5-revolution absurd-value guard in `Rpy` runs after wrapping, so it never fires: [MAY-90: Need: Rpy rejects an angle above 5 revolutions](https://linear.app/mayday-robot/issue/MAY-90).
- `Ray3` and `Vector3Extensions` are declared in namespace `RobotDomain.Geometry`, not `RobotDomain.Geometry.SystemNumerics`: [MAY-95: Need: Every type in Geometry/SystemNumerics is in namespace RobotDomain.Geometry.SystemNumerics](https://linear.app/mayday-robot/issue/MAY-95).
- `Xyz.DistanceBetween(a, b)` is an instance method that ignores the instance it is called on.

## Side Effects

- `Q.Random`, `Rpy.Random` and `Xyz.Random` create a new unseeded `System.Random` per call, so results are not reproducible.
- No I/O, logging, threads or mutable static state.

## Verification

`dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.RobotDomain.Geometry"`

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
