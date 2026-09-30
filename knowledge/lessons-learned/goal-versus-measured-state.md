# Separate trajectory goals from measured joint state

## Situation

The joint driver interpolates commanded angles while hardware may lag behind the commanded position.

## Observation

The 2026-01-02 entry in [DevLog.md](../../DevLog.md) reports that using the current angle as each new baseline made short steps unreliable and incurred extra IO. The current `JointState` stores both measured `Angle` and timed `AngleGoalPrevious`/`AngleGoal`.

## Recommendation

Keep commanded trajectory history separate from sensor readings; use the sensor state for feedback, not as an implicit replacement for the previously commanded goal. Review timing and motor feedback together before changing interpolation.

## Related Components

[RobotDomain/Structures](../../RobotDomain/Structures/README.md), [RobotDomain/Motion](../../RobotDomain/Motion/README.md)

## Related Tasks

[Repository memory foundation](../task-summaries/repository-memory-foundation.md) records this historical lesson.
