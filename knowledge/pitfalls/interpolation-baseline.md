# Pitfall

## Description

Re-baselining each timed goal on a measured angle instead of the last commanded goal.

## Why It Is Dangerous

Motor lag can cause short steps to target the wrong direction; extra synchronous reads also consume communication time. See the [diagnosed root cause](../root-causes/interpolation-baseline-drift.md).

## Warning Signs

Interpolation code derives its starting angle from `ReadAngle` or `JointState.Angle` for each step rather than `AngleGoalPrevious.Target`.

## Preferred Approach

Keep sensor feedback and planned goal history separate, and test with a delayed/lagging joint.

## Related Components

[RobotDomain/Structures](../../RobotDomain/Structures/README.md), [RobotDomain/Motion](../../RobotDomain/Motion/README.md)
