# Q.Rotate rejected positions longer than 1.1 m

## Symptoms

`Link.GetTransformOf` threw `ArgumentException: Cannot create Q with absurd values` for an attachment offset of `(1, 2, 3)` m. Three tests were quarantined with "Q rejects the test input as absurd values (abs(v) > 1.1)", which pointed at the test input rather than the code.

## Investigation

The stack trace of `ComponentAttachmentTests.GivenBaseLinkAndAttachedLink_WhenGetTransformOfChildId_ThenReturnsTransformOfAttachment` ended in `Q.Rotate` (called from `Transform.Add`), constructing `new Q(0, 1, 2, 3)`. The rotation `Q.FromRpy(...)` in the test was valid. The other two `Q` validation tests (`QTests`) called `Rotate` on `(1, 2, 3)`, or built `new Q(2, 0, 0, 0)` directly.

## Root Cause

`Q.Rotate` computed `q * p * q^-1` by wrapping the position `p` as a pure quaternion `Q(0, x, y, z)`. A `Q` is a rotation and validates its components to be at most 1.1, so any coordinate beyond that failed. `Xyz` had its own 5 m limit, which would also have rejected larger results. The identity-rotation test did not fail because of the rotation but because of this wrapping.

## Resolution

`Q.Rotate` rotates with `Vector3.Transform(Vector3, Quaternion)` and no longer builds a `Q` from a position (`cad4918`). The `Xyz` 5 m limit was removed (`515c07a`). The non-unit test used `new Q(2, 0, 0, 0)`, which `Q` correctly rejects, so its input became `new Q(0.5, 0, 0, 0)`.

## Prevention

- A position is never a `Q`. Keep `Q` validation strict, and rotate vectors through `Q.Rotate`.
- `Q` is a facade over `System.Numerics.Quaternion` and computes in `float` (the BCL has no double quaternion), so compare with `IsRotationAlmostEqual` and `Xyz.IsAlmostEqual`, not exact equality.
- `QTests.Rotate_PointFurtherThanOneMeter_RotatesPointCorrectly` guards the bug.

## Related Components

[Geometry](../../RobotDomain/Geometry/README.md)

## Related Tasks

[Gate pull requests and establish a test baseline](../task-summaries/pr-gating-and-test-baseline.md)

## Related Decisions

[ADR 0004](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md)
