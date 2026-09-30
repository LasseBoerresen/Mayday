# Shared robot structures

## Responsibility

Model links, attachments, joints, wheel state, and composed structures.

## Public APIs

`Structure`, `Link`, `Connection`, `Joint`, `JointFactory`, `JointState`, `EchoJoint`.

## Key Concepts

Connection order, transform graph, measured joint state versus timed goal history.

## Dependencies

Shared geometry, physics, timing; hardware factories supplied by callers.

## Dependency Rules

Keep specific leg/wheel layouts in Mayday/Ellie projects.

## Invariants

`JointState.InterpolateGoalAngleOneTimeStep` starts at the previous commanded goal, not the measured angle.

## Architectural Constraints

Joint IDs, orientation, and graph ordering affect kinematics and physical motion.

## Common Pitfalls

[Measured-angle baseline](../../knowledge/pitfalls/interpolation-baseline.md).

## Related ADRs

[Stable abstractions](../../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

[Interpolation drift](../../knowledge/root-causes/interpolation-baseline-drift.md).

## Related Lessons Learned

[Goal versus measured state](../../knowledge/lessons-learned/goal-versus-measured-state.md).

## Related Failed Attempts

[Measured-angle interpolation](../../knowledge/failed-attempts/measured-angle-interpolation.md).
