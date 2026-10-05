# Pitfall

## Description

`Q`'s binary `-` operator returns `inverse(a) * b`, the rotation from `a` to `b`. `Q`'s `+` returns `a * b`, so `b + (a - b)` is not `a`. The operator is the reverse of what `Transform.Subtract` documents ("from b to a") and of the translation it computes.

## Why It Is Dangerous

A caller who treats `-` as the inverse of `+` gets a rotation in the wrong direction without any exception. It caused the wrong result in `Transform.HalfWayTo` (see the [root cause](../root-causes/transform-halfway-to-wrong-result.md)).

## Warning Signs

Code computing a relative rotation with `a.Q - b.Q`, or a rotation delta whose angle is the opposite of the expected one.

## Preferred Approach

Use `Q.Inverse(b) + a` for the rotation from `b` to `a`, and verify with `b + (a - b) == a`. The operator currently has no callers; change its meaning only with a test for the invariant above.

## Related Components

[Geometry](../../RobotDomain/Geometry/README.md)
