# Mayday components

## Overview

Everything about Mayday's physical components (namespace
`MaydayDomain.Components`). A component need not be hardware itself: the
namespace is meant to span one coherent set of types that are about the
physical parts of the robot, including their identity, state and assembly.

Today it holds only the thorax and the leg sections, with their mount
geometry. The rest still lives at the [MaydayDomain](../README.md) root (see
Planned scope).

## Responsibility

Model Mayday's physical components and the types focused on them. Currently: hexapod link dimensions and mounting geometry.

## Planned scope

These types are about the components and belong here, but still live at the
`MaydayDomain` root. Moving them is an idea, not done and not scheduled. It is
part of the [planned namespace layout](../README.md#planned-namespace-layout).

| Types | How they relate to the components |
|---|---|
| `MaydayStructure`, `DefaultMaydayStructure` | The whole robot the components form. |
| `MaydayLeg` | A component itself, made of the leg sections here. |
| `JointLimits` | A physical property of the coxa, femur and tibia joints, like the mount geometry. |
| `MaydayLegFactory` | Assembles a leg from its components, and holds part of their geometry (see Invariants). |
| `EchoJointFactory` | Supplies the joints of a leg when there is no hardware. |
| `LegPostureByPositionMap`, `LegPostureByPositionMapDictImpl`, `LegPostureByPositionMapRepo` | The leg posture that puts a tip at a position. |

The leg IDs, the per-leg sets and the postures are about the components too,
but nearly every type in the project uses them, so they fit a base namespace
better (see the planned layout). `MotionPlanning` and `Internal` stay separate
namespaces.

## Public APIs

`Coxa`, `Femur`, `Tibia`, `Thorax`.

## Interface

All types are in namespace `MaydayDomain.Components`. Translations are in
meters and yaws in revolutions: the values go through the
`Xyz(double, double, double)` and `Rpy(double, double, double)` constructors.

Where the values come from:

- The motor and tibia mount translations of the leg sections are taken from
  the CAD model.
- The thorax leg mounts are measured coarsely on the real robot with a caliper
  and are only approximate. That is enough for now: Mayday is a flexible robot
  that needs to move organically, not a high-accuracy one.
- Planned: take the thorax mounts from the CAD model once it is in source
  control.

### Leg sections

Each is a record with one static mount translation.

| Type | Member | Value | File |
|---|---|---|---|
| `Coxa` | `FemurMotorMountTranslation` | `(0.033, 0.0, -0.013)` | `Coxa.cs` |
| `Femur` | `TibiaMotorMountTranslation` | `(0.079047, 0.0, 0.032494)` | `Femur.cs` |
| `Tibia` | `TibiaMountTranslation` | `(-0.017, 0.025, 0)` | `Tibia.cs` |

### Thorax

#### `Thorax`

The body the six legs mount on. Record. File: `Thorax.cs`.

Leg mounts, each a static `Transform` made of a translation and a yaw:

| Property | Translation | Yaw |
|---|---|---|
| `LeftFrontTransform` | `(0.085, 0.075, 0)` | `0.125` |
| `LeftCenterTransform` | `(0.000, 0.100, 0)` | `0.25` |
| `LeftBackTransform` | `(-0.085, 0.075, 0)` | `0.375` |
| `RightFrontTransform` | `(0.085, -0.075, 0)` | `-0.125` |
| `RightCenterTransform` | `(0.000, -0.100, 0)` | `-0.25` |
| `RightBackTransform` | `(-0.085, -0.075, 0)` | `-0.375` |

Other members:

| Member | What it does |
|---|---|
| `TransformFor(legId)` | The mount of that leg. Throws `ArgumentOutOfRangeException` for an ID that is not one of the six `MaydayLegId` values. |
| `ViewedAs(legId)` | A function that converts a transform in the thorax frame to the same transform in the leg's own frame: `t - TransformFor(legId)`. Needed because a tip goal is given in the thorax frame, while a leg sets its tip position in its own frame; `DefaultMaydayStructure.MoveTipsTo` converts each goal this way before handing it to the leg. |
| `XyzViewedAs(legId)` | The same for a position. |
| `Origin` | `Transform.Zero`. The only instance member. |

#### `ThoraxExtensions`

Shorthand for the two conversions above. File: `Thorax.cs`.

| Method | What it does |
|---|---|
| `xyz.ViewedFrom(legId)` | Applies `Thorax.XyzViewedAs(legId)`. |
| `transform.ViewedFrom(legId)` | Applies `Thorax.ViewedAs(legId)`. |

## Key Concepts

Physical leg sections used by Mayday structure and kinematics.

## Dependencies

Part of `MaydayDomain`; shared geometry from `RobotDomain`.

## Dependency Rules

Do not move Mayday-specific dimensions into generic robot geometry.

## Invariants

Mount translations and orientations must agree with `MaydayLegFactory`.

Mayday is mirrored left to right, like the spider it resembles: a right mount is the left mount with `Y` and yaw negated. Keep the two sides in step when changing a mount.

`MaydayLegFactory` holds the tibia-to-tip transform `(0.125, 0, -0.09)` and the orientations of all joints and attachments as inline literals. They are component geometry and belong here.

## Side Effects

- None. Every member is a pure value or function.

## Architectural Constraints

Geometry changes affect reachable postures and physical safety.

## Common Pitfalls

Do not change a measurement without checking kinematics and joint limits.

The front and back mounts are yawed 0.125 revolutions (45 degrees), which is believed to be the real angle. Their positions lie at about 41.4 degrees from the thorax origin, so they do not face exactly away from it, although the XML doc of `MotionPlanning.Stance` says each mount does. Check both against the CAD model once it is in source control.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDomain.Components|FullyQualifiedName~Test.Unit.Components.ThoraxTests"`
  (the `ViewedFrom` tests are in `Test.Unit.Components.ThoraxTests`, outside the `MaydayDomain` test namespace).

## Related ADRs

[Stable abstractions](../../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
