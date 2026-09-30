# System.Numerics geometry adapters

## Responsibility

Provide numerics-based ray, triangle, and vector operations for shared geometry.

## Public APIs

`Ray3`, `Triangle3`, `Vector3Extensions`.

## Key Concepts

Interop with `System.Numerics` spatial primitives.

## Dependencies

`RobotDomain.Geometry` and `System.Numerics`.

## Dependency Rules

Keep robot-specific body dimensions outside this namespace.

## Invariants

Preserve frame and unit assumptions when converting coordinates.

## Architectural Constraints

Check conversions against units-aware `Xyz`/`Transform` APIs.

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
