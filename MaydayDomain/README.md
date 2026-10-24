# MaydayDomain

## Responsibility

Mayday's hexapod structure, leg geometry, posture, limits, kinematics, and motion planning.

## Public APIs

`MaydayLeg`, `MaydayStructure`, their factories, `LegPostureByPositionMap`, and [motion planners](MotionPlanning/README.md).

## Key Concepts

Six leg IDs, coxa/femur/tibia links, joint limits, posture-to-tip position mapping.

## Dependencies

`RobotDomain` and currently `Dynamixel` in `MaydayDomain.csproj`.

## Dependency Rules

Keep shared robot concepts in `RobotDomain`; trace the existing hardware reference before changing it.

## Invariants

Preserve leg IDs, orientation conventions, dimensions, joint limits, and units.

## Architectural Constraints

The direct Dynamixel project reference is a [documented exception](../knowledge/findings/mayday-domain-hardware-reference.md) to the intended direction.

## Common Pitfalls

Do not infer physical orientation or safe angles from numeric IDs alone; test using echo joints first. See [posture lookups that silently keep the current posture](../knowledge/pitfalls/posture-map-silent-fallback.md).

## Related ADRs

[Stable abstractions](../docs/adr/0001-stable-robot-abstractions.md), [hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md).

## Related Findings

[Hardware reference](../knowledge/findings/mayday-domain-hardware-reference.md), [Scheduler construction](../knowledge/findings/scheduler-constructed-inside-domain-classes.md), [Posture map accuracy](../knowledge/findings/posture-map-accuracy.md), [Right coxa direction mismatch](../knowledge/findings/right-coxa-direction-mismatch.md)

## Related Root Causes

[Interpolation baseline drift](../knowledge/root-causes/interpolation-baseline-drift.md), [Empty leg posture map](../knowledge/root-causes/empty-leg-posture-map.md), [Rear femur hits the body](../knowledge/root-causes/rear-femur-hits-body.md)

## Related Lessons Learned

[Goal versus measured state](../knowledge/lessons-learned/goal-versus-measured-state.md), [Predict on echo before hardware](../knowledge/lessons-learned/predict-on-echo-before-hardware.md).

## Related Failed Attempts

[Measured-angle interpolation](../knowledge/failed-attempts/measured-angle-interpolation.md).
