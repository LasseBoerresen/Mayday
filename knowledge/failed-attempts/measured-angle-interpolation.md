# Goal

Pace short joint movements accurately while motors can lag under load.

## Attempt

Use the current measured angle as the baseline for each interpolated step.

## Why It Failed

Small initial steps could target an angle behind the previous goal when the actuator had not caught up; each read also added hardware communication overhead.

## Evidence

[DevLog.md](../../DevLog.md), 2026-01-02 entry, reports the failure and result of switching baselines. `RobotDomain/Structures/JointState.cs` now interpolates from the previous commanded target.

## Lessons Learned

Command history and sensor state represent different things; see [the lesson](../lessons-learned/goal-versus-measured-state.md) and [root cause](../root-causes/interpolation-baseline-drift.md).

## Preferred Alternative

Interpolate between `AngleGoalPrevious.Target` and `AngleGoal.Target`; use measured angle for feedback independently.

## Related Components

[RobotDomain/Structures](../../RobotDomain/Structures/README.md), [RobotDomain/Motion](../../RobotDomain/Motion/README.md)

## Related Decisions

[ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
