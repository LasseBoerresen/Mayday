# System.Numerics geometry adapters

## Responsibility

Provide unitless `float` ray, triangle and vector operations on `System.Numerics` primitives. They are implementation helpers, not a public API.

## Interface

- `Ray3(Vector3 Origin, Vector3 Direction)`: `float` ray (`Ray3.cs`).
- `Triangle3(Vector3 v0, Vector3 v1, Vector3 v2)`: `float` triangle (`Triangle3.cs`).
  - `TryGetIntersection(in Ray3, out Vector3 intersectionPoint, out float distance)`: ray-triangle hit test; on a hit, `distance` is the ray parameter `t` and the point is `Origin + Direction·t`.
  - `IsColinear()`, `Area()`, `Center()`.
- `Vector3Extensions.Random(float max = 1)`: random vector, each component in [−max, max], not normalized; `ToShortString(this Vector3)` (`Vector3Extensions.cs`).

## Key Concepts

- **Unitless:** every value is a plain `float`. The caller decides what unit a coordinate is in and owns any conversion to or from unit types.
- **Parametric distance:** a ray's `Direction` is not normalized, so the `distance` of a hit is the ray parameter `t`, a length only when `Direction` has unit length.

## Dependencies

- `System.Numerics` (`Vector3`) for all values and math.
- `Generic` for the random helper (`Vector3Extensions.Random`).

## Dependency Rules

Keep robot-specific body dimensions outside this namespace.

## Invariants

Preserve frame and unit assumptions when converting coordinates.

- `TryGetIntersection` does no backface culling (a hit from either side counts), treats edges and vertices as hits, and requires `t` > 1e-9 (strictly in front of the origin). On a miss it returns `false` with default outputs.
- `IsColinear` uses a fixed absolute tolerance: cross-product length below 1e-2.
- No validation or exceptions; `Ray3` and `Triangle3` are readonly record structs.

## Known Issues

- `Ray3` and `Vector3Extensions` are declared in namespace `RobotDomain.Geometry`, not `RobotDomain.Geometry.SystemNumerics`: [MAY-95: Need: Every type in Geometry/SystemNumerics is in namespace RobotDomain.Geometry.SystemNumerics](https://linear.app/mayday-robot/issue/MAY-95). As implementation helpers, these types are also a candidate for an `Internal` namespace.
- `Triangle3.EPSILON` is the `double` 1e-9, compared with `float` values that resolve only about 1e-7. In practice `TryGetIntersection` rejects only exactly parallel rays and hits at the origin; nearly parallel rays still count as hits, with numerically unstable results: [MAY-100: Need: Triangle3.TryGetIntersection rejects nearly parallel rays with a float-sized tolerance](https://linear.app/mayday-robot/issue/MAY-100).
- `IsColinear`'s tolerance is absolute, not relative to the triangle's size. With coordinates in meters, any triangle smaller than 50 cm² counts as colinear, for example a right triangle with 5 cm legs (12.5 cm²): [MAY-101: Need: Triangle3.IsColinear judges colinearity relative to the triangle's size](https://linear.app/mayday-robot/issue/MAY-101).

## Side Effects

- `Vector3Extensions.Random` creates a new unseeded `System.Random` per call.
- No I/O, logging, threads or mutable static state.

## Verification

`dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.RobotDomain.Geometry"`

## Architectural Constraints

Values carry no units, so a conversion to or from unit types belongs to the caller that makes it.

## Common Pitfalls

Unlabeled `Vector3` values can hide unit mismatches.

## Related ADRs

[Stable abstractions](../../../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
