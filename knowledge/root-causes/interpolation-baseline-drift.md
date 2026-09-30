# Interpolation drift from a measured-angle baseline

## Symptoms

Small interpolated commands could target a position behind the previous goal when the motor had not reached it yet.

## Investigation

[DevLog.md](../../DevLog.md) (2026-01-02) reports motor lag from friction/load and notes that reading the current angle for every new goal both changed the baseline and cost communication time. Current `RobotDomain/Structures/JointState.cs` computes the difference between `AngleGoal.Target` and `AngleGoalPrevious.Target`.

## Root Cause

Using the measured motor angle as the start of each interpolated goal conflated actual state with the planned trajectory. A motor behind its goal makes the next tiny step move relative to the wrong starting point.

## Resolution

The development log records switching to the previous commanded goal as the interpolation baseline. `JointState.InterpolateGoalAngleOneTimeStep` now interpolates from `AngleGoalPrevious.Target` toward `AngleGoal.Target`.

## Prevention

Preserve separate measured and commanded state; test trajectories with a joint that lags behind its target. Do not assume tuning alone fixes a faulty interpolation baseline.

## Related Components

[RobotDomain/Structures](../../RobotDomain/Structures/README.md), [RobotDomain/Motion](../../RobotDomain/Motion/README.md)

## Related Tasks

[Repository memory foundation](../task-summaries/repository-memory-foundation.md) documents the historical incident; the code fix predates this task.

## Related Decisions

[ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
