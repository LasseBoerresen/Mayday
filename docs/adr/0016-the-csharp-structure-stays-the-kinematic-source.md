# The C# structure stays the kinematic source

## Status

Accepted.

## Context

Collision detection and simulation will need Mayday's geometry. Today the C# structure (links, joints, dimensions in `MaydayDomain` and `RobotDomain/Structures`) defines the kinematics, while the shapes live in Fusion 360 ([ADR 0015](0015-fusion-is-the-cad-source-the-repository-holds-exports.md)). The two have already disagreed: the [right coxa direction](../../knowledge/findings/right-coxa-direction-mismatch.md), and a [rear femur that hit the body](../../knowledge/root-causes/rear-femur-hits-body.md), which a collision check would have caught.

## Decision

- The C# structure remains the source of link lengths, joint origins and axes.
- Collision checks will use simple shapes (capsules, boxes) attached to the C# links, not CAD meshes. Meshes, when exported, are for visualization.
- A test will compare the C# dimensions with values measured from the CAD model, so drift between them fails.
- When a simulator is chosen, the source moves to a robot description file (URDF or MJCF) in the repository that both C# and the simulator load; that move gets its own ADR.

## Alternatives Considered

- CAD as the source, exported to URDF (fusion2urdf) and loaded by C#: one source and exact shapes, but the exporter is fragile, the Fusion model must be built around correct joints and components, and mesh collision is slow enough that simple shapes are needed anyway.
- A description file as the source now: diffable and ready for simulators, but it needs a C# loader before anything uses it.

## Why

The C# structure already exists, is tested and uses UnitsNet types; nothing consumes another format yet. Simple shapes are what collision checks need for speed, and a dimension test turns the known weakness, drift from CAD, into a failing test instead of a surprise on the robot.

## Consequences

- Benefits: no new toolchain; collision checks can be built on existing types.
- Costs: dimensions exist in two places; collision shapes are maintained by hand.
- Limitations: no simulator can load the structure until the description-file step. Neither the collision shapes nor the dimension test exist yet.

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [RobotDomain/Structures](../../RobotDomain/Structures/README.md), [hardware/mayday](../../hardware/mayday/README.md)

## Related ADRs

[0001](0001-stable-robot-abstractions.md), [0013](0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md), [0015](0015-fusion-is-the-cad-source-the-repository-holds-exports.md)

## Related Findings

[Right coxa direction mismatch](../../knowledge/findings/right-coxa-direction-mismatch.md)

## Related Root Causes

[Rear femur hits the body](../../knowledge/root-causes/rear-femur-hits-body.md)
