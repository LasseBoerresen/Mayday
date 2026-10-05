# Transform.HalfWayTo returned the wrong transform

## Symptoms

`TransformTests.GivenNonZeroTransform__WhenCalculateHalfwayToZero__ThenShouldBeHalfway` was quarantined with "Assertion mismatch on the halfway transform; cause not yet diagnosed." Halfway from `(0.2, 0.4, -0.6)` with a 0.2 revolution rotation to `Transform.Zero` was expected to be `(0.1, 0.2, -0.3)` with 0.1 revolutions.

## Investigation

Three causes were stacked, and each was only visible after removing the one before it:

1. Before any change, the expected quaternion printed as `W 0.951, X 0.309, Z 0.309`, which has norm above 1. The test built it with `Q.FromAxisAngle(new(1, 0, 1), ...)`, and `Quaternion.CreateFromAxisAngle` requires a unit axis. Normalizing the axis made the expected value valid but the test still failed: actual angle 0.5 revolutions, translation `(0.027, 0.731, -0.627)`.
2. `Transform.Subtract` is documented as "the transform from b to a", but computed its rotation with `Q`'s `-` operator, which is `inverse(a) * b`. The correct value is `inverse(b) * a`, so that `b + (a - b) == a`. Fixing this changed the actual angle from 0.5 to 0.3 revolutions; the translation did not change.
3. `HalfWayTo` returned `this + InDirectionTo(other, 0.5)`, but `InDirectionTo` already returns `this + DistanceTo(other) * factor`, so the start was applied twice. This explained the unchanged translation. After the fix the test passed.

## Root Cause

An invalid test input hid two production bugs in [Transform.cs](../../RobotDomain/Geometry/Transform.cs): the start transform was added twice in `HalfWayTo`, and `Subtract` used a rotation difference in the opposite direction of its documented contract and of its own translation.

## Resolution

Commit `5aa4f3e`: the test normalizes its axis and is un-quarantined; `HalfWayTo` returns `InDirectionTo(other, 0.5)`; `Subtract` uses `Q.Inverse(b.Q) + a.Q`. `Q`'s `-` operator is unchanged, since `Subtract` was its only caller (see the [pitfall](../pitfalls/q-minus-operator-direction.md)).

## Prevention

- Pass a normalized axis to `Q.FromAxisAngle`.
- A quarantined assertion mismatch can hide several defects. Fix the test input first, rerun, and read the new actual value before deciding the production code is right or wrong.
- `HalfWayTo`, `InDirectionTo` and `-` have a single test for the halfway case only. A test of the invariant `b + (a - b) == a` would catch a direction error directly.

## Related Components

[Geometry](../../RobotDomain/Geometry/README.md)

## Related Tasks

None recorded.

## Related Decisions

[ADR 0004](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md)
