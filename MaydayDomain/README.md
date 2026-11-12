# MaydayDomain

## Overview

The domain layer of the Mayday robot, in the sense of clean architecture and
domain-driven design. It holds the robot's core model and behavior, and is
independent of infrastructure and persistence.

## Responsibility

Mayday's hexapod structure, leg geometry, posture, limits, kinematics, and motion planning.

## Public APIs

`MaydayLeg`, `MaydayStructure`, their factories, `LegPostureByPositionMap`, and [motion planners](MotionPlanning/README.md).

## Interface

All types are in namespace `MaydayDomain`. Each heading below is one type; the
line under it says what the type is for, and the table lists what a caller can
do with it.

### Subdirectories

| Folder | Holds |
|---|---|
| [Components](./Components/README.md) | Link dimensions and thorax mounts: `Coxa`, `Femur`, `Tibia`, `Thorax`. |
| [MotionPlanning](./MotionPlanning/README.md) | `Motion`, `Stance`, the motion planners and the experimental learned inverse leg kinematics. |
| [Internal](./Internal/README.md) | Implementation details: `MirroredInYLegPostureByPositionMap`. |

### Identifying legs

#### `MaydayLegId`

Names one of the six legs by its side and its position along that side.
Record, comparable. File: `MaydayLegId.cs`.

| Member | What it does |
|---|---|
| `LeftFront`, `LeftCenter`, `LeftBack`, `RightFront`, `RightCenter`, `RightBack` | The six legs. |
| `AllLegIds` | All six, in the order above. |
| `Value()` | A number per leg: `Side * 9 + SidePosition * 3`. |
| `JointId(jointNumber)` | The ID of one of the leg's joints: `Value() + jointNumber`. |
| `CompareTo(other)` | Orders legs by `Value()`. |

#### `SidePosition`

Where a leg sits along its side. Enum: `Front` (0), `Center` (1), `Rear` (2).
File: `SidePosition.cs`.

### One value per leg

#### `MaydayStructureSet<T>`

Holds one value for each leg, for example six tip positions. Record with the
fields `RF`, `RC`, `RB`, `LF`, `LC`, `LB`; it enumerates in that order.
File: `MaydayStructureSet.cs`.

| Member | What it does |
|---|---|
| `FromSingle(value)` | The same value for every leg. |
| `FromLegDict(dictionary)` | From a dictionary keyed by `MaydayLegId`. |
| `FromEnumerable(values)` | From six values, taken in set order. |
| `FromLegProperties(properties)` | From six `LegProperty<T>`, taken in set order. |
| `ToLegDict()`, `ToLegProperties()` | The values back, with their leg IDs. |
| `Map(mapper)` | A new set with each value converted. A second overload converts `LegProperty<T>`, so the mapper sees the leg ID. |
| `CombineWith(other, combiner)` | A new set from two sets, leg by leg. |
| `this[legId]` | The value of one leg. |

#### `LegProperty<T>`

A value together with the leg it belongs to. Record `(LegId, Value)` with
`Map(mapper)`. File: `LegProperty.cs`.

#### Extension methods

| Method | What it does | File |
|---|---|---|
| `ToMaydayStructureSet()` | Builds a set from a leg dictionary, from `LegProperty<T>` values or from a plain list. | `MaydayStructureSetExtensions.cs` |
| `Sequence()` | Turns one timed set into a set of timed values. | `MaydayStructureSet.cs` |

### Postures and limits

#### `MaydayLegPosture`

The three joint angles of one leg. Record
`(CoxaAngle, FemurAngle, TibiaAngle)`. File: `MaydayLegPosture.cs`.

| Member | What it does |
|---|---|
| `FromRevolutions(coxa, femur, tibia)` | A posture from three numbers in revolutions. |
| Constructor from `IEnumerable<Angle>` | Takes the first three angles. |
| `FromSines(...)` | A posture from the sine and cosine of each angle. |
| `Neutral`, `NeutralWithStraightFemur`, `NeutralWithBackTwist`, `Straight`, `StraightWithBackTwist`, `Sitting`, `SittingTall`, `Standing`, `StandingHigh`, `StandingWide` | Named postures. |
| `AsListOfGoalAngles()` | The angles as a list: coxa, femur, tibia. |
| `a - b` | The difference per joint. |
| `DistanceTo(other)` | How far apart two postures are, as one angle. |
| `InterpolateBetween(start, end, fraction)` | A posture part of the way from `start` to `end`, in revolutions. Throws `ArgumentException` when `fraction` is outside 0 to 1. |

#### `MaydayStructurePosture`

The postures of all six legs. A `MaydayStructureSet<MaydayLegPosture>` with
`FromSingle`, `FromSet` and `ToSet`. File: `MaydayStructurePosture.cs`.

#### `JointLimits`

The allowed angle range of each joint. Record
`(CoxaMin, CoxaMax, FemurMin, FemurMax, TibiaMin, TibiaMax)`.
File: `JointLimits.cs`.

| Member | What it does |
|---|---|
| `Defaults` | Coxa ±0.125 revolutions; femur and tibia ±0.4 revolutions. |

### Legs

#### `MaydayLeg`

One leg: seven links, three joints, and a map for finding the posture that
puts the tip at a position. File: `MaydayLeg.cs`.

Reading the leg:

| Member | What it does |
|---|---|
| `Length` | 0.26 m. |
| `JointCount` | 3. |
| `BaseLink` | The coxa motor link, where the leg attaches to the thorax. |
| `GetPosture()` | The current angle of the three joints. |
| `GetTipPosition()` | Where the tip is, seen from the coxa motor link. |
| `GetTransformOf(linkName)`, `GetTransformOf(link)` | Where a link is, seen from the coxa motor link. |
| `LinkFromName(linkName)` | The link with that name. Throws `NotSupportedException` for a name the leg does not have. |

Moving the leg:

| Member | What it does |
|---|---|
| `SetPosture(posture)` | Sets the goal angle of each joint. Virtual. |
| `SetTipPositionTo(tipPosition)`, `MoveTipPositionTo(tipPosition)` | Look up the posture for that tip position, closest to the current posture, and set it. |
| `MoveTipPositionBy(tipOffset)` | The same, for the current tip position plus an offset. |

Static helper:

| Member | What it does |
|---|---|
| `ApplyForJointAngleRanges(action, angleStep)` | Calls `action` for every posture on a grid inside `JointLimits.Defaults`, from each minimum (included) to each maximum (excluded). |

#### `MaydayLegFactory`

Builds legs from a `JointFactory` and a posture map made for a left leg.
File: `MaydayLegFactory.cs`.

| Member | What it does |
|---|---|
| `CreateLeg(legId)` | Builds one leg: seven links, three joints with the IDs `legId.JointId(1)` to `JointId(3)`, three fixed attachments. |
| `CreateAll()` | One leg for each of `MaydayLegId.AllLegIds`. |
| `NewEchoLegFactory()` | A factory for hardware-free legs: echo joints and the neutral map. |

Left and right legs differ in two ways:

- A left coxa turns `Forward`, a right coxa `Reverse`. Femur and tibia joints
  always turn `Forward`.
- A right leg gets the map wrapped in
  `Internal.MirroredInYLegPostureByPositionMap`.

#### `EchoJointFactory`

A `JointFactory` for hardware-free use. `New(...)` returns a `RobotDomain`
`EchoJoint` with a new `ComponentId`; it ignores the `JointId`.
File: `EchoJointFactory.cs`.

### The whole robot

#### `MaydayStructure`

The interface to the robot's body: ask where its parts are, and tell its legs
where to go. File: `MaydayStructure.cs`.

Queries:

| Member | What it returns |
|---|---|
| `GetPositionsOf(linkName)`, `GetPositionOf(linkName, legId)` | The position of a link, for every leg or for one. |
| `GetOrientationsOf(linkName)`, `GetTransformsOf(linkName)` | The orientation or full transform of a link, for every leg. |
| `GetPostures()`, `GetPostureOf(legId)` | The current posture of every leg, or of one. |
| `GetCurrentLean()` | The pose of the thorax relative to the ground. |

Commands:

| Member | What it does |
|---|---|
| `SetPosture(posture)` | Sets a posture per leg. |
| `SetPostureForAllLegs(posture)` | Sets the same posture on every leg. |
| `MoveTipsTo(tipPositions)` | Moves each tip to a position. |
| `MoveTipsBy(offset)` | Moves every tip by the same offset. |
| `SetStance(motion)` | Places every tip at its footprint for the motion's lean and stance radius. |
| `MoveThoraxTo(motion)` | Leans the thorax by moving the tips the other way. |

#### `DefaultMaydayStructure`

The implementation of `MaydayStructure`: a thorax link and six `MaydayLeg`s.
File: `DefaultMaydayStructure.cs`.

| Member | How it behaves |
|---|---|
| Link queries | Return positions and transforms seen from the thorax link. |
| `MoveTipsTo` | Takes positions in the thorax frame, converts each to its leg's frame and calls `MaydayLeg.MoveTipPositionTo`. |
| `MoveTipsBy` | `MoveTipsTo` the current tip positions plus the offset. An extra overload takes one offset per leg. |
| `SetStance` | Absolute: `MoveTipsTo` the positions from `Stance.GetTipPositionsInThoraxFrameFor`. Repeating the same motion asks for the same tip positions, so it cannot accumulate drift. Whether the legs then end in the same postures is not verified: the posture lookup depends on each leg's current posture. Ignores the twist. |
| `MoveThoraxTo` | Incremental: moves the tips opposite to the difference between the requested lean's translation and that of `GetCurrentLean()`, and further down in Z by each tip's clearance from the estimated ground plane. Does not apply the lean's rotation. |
| `GetCurrentLean()` | Translation: minus the mean tip X and Y, and the ground clearance in Z. Rotation: always `Q.Unit`. |

#### `MaydayStructureFactory`

Assembles a `DefaultMaydayStructure` from a `MaydayLegFactory`.
File: `MaydayStructureFactory.cs`.

| Member | What it does |
|---|---|
| `CreateDefault()` | Creates a thorax link and six legs, and attaches each leg's `BaseLink` to the thorax at `Thorax.TransformFor(legId)`. |
| `CreateEcho()` | `CreateDefault()` with `MaydayLegFactory.NewEchoLegFactory()`: a hardware-free structure. |

### Finding the posture for a tip position

#### `LegPostureByPositionMap`

The interface for inverse kinematics by lookup. File:
`LegPostureByPositionMap.cs`.

| Member | What it does |
|---|---|
| `GetFor(tipPosition, currentPosture)` | Returns the posture that puts the tip at `tipPosition`. When several do, the one closest to `currentPosture`. Documented to throw `InvalidOperationException` for a position the leg cannot reach (see Common Pitfalls). |

#### `LegPostureByPositionMapDictImpl`

The lookup as a grid: space is cut into cells of 1/64 m, and each cell lists
the postures whose tip lands in it. Record. File:
`LegPostureByPositionMapDictImpl.cs`.

| Member | What it does |
|---|---|
| `Map` | The cells. Setting an empty dictionary throws `EmptyMapException` (a nested class). |
| `GetFor(tipPosition, currentPosture)` | Finds the cell of the tip position and its closest neighbor cell, takes from each the posture closest to `currentPosture`, and interpolates between the two. |
| `BuildNew()` | Computes a full map: sets every posture from `MaydayLeg.ApplyForJointAngleRanges` (step 1/64 revolution) on an echo `LeftBack` leg and records where the tip ends up. |
| `CreateNeutral()` | A map with one entry, the origin to `MaydayLegPosture.Neutral`, for legs that need no real lookups. |

#### `LegPostureByPositionMapRepo`

The interface for storing a map: `Load()` and
`Store(LegPostureByPositionMapDictImpl)`. Implemented outside this project.
File: `LegPostureByPositionMapRepo.cs`.

## Key Concepts

Six leg IDs, coxa/femur/tibia links, joint limits, posture-to-tip position mapping.

## Dependencies

`RobotDomain` and currently `Dynamixel` in `MaydayDomain.csproj`. No source file in this project names `Dynamixel`. The TensorFlow packages serve [MotionPlanning](./MotionPlanning/README.md); `Microsoft.ML` and the ONNX packages are referenced but no source file uses them.

## Dependency Rules

Keep shared robot concepts in `RobotDomain`; trace the existing hardware reference before changing it.

The domain declares the contracts it needs and outer layers implement them: actuators come in as a `JointFactory` (implemented by `Dynamixel`, or by `EchoJointFactory` for hardware-free use), and stored posture maps through `LegPostureByPositionMapRepo` (implemented by `MaydayDataAccess`). Composition roots wire the implementations in.

## Invariants

Preserve leg IDs, orientation conventions, dimensions, joint limits, and units.

Right legs are the mirror image of left legs: their coxas turn in reverse, and `MaydayLegFactory` gives them the left-leg posture map through `Internal.MirroredInYLegPostureByPositionMap`, which looks up the target mirrored in Y of the leg's frame ([ADR 0013](../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md)).

Joint IDs follow from `MaydayLegId.Value()`: `LeftFront` 1 to 3, `LeftCenter` 4 to 6, `LeftBack` 7 to 9, `RightFront` 10 to 12, `RightCenter` 13 to 15, `RightBack` 16 to 18, each in coxa, femur, tibia order.

A leg's links are ordered coxa motor, coxa, femur motor, femur, tibia motor, tibia, tip, and its joints coxa, femur, tibia. `MaydayLeg` reads both by position.

`MaydayStructureSet<T>` orders legs RF, RC, RB, LF, LC, LB. `FromEnumerable` and `FromLegProperties` assign by position in that order, while `MaydayLegId.AllLegIds` lists left legs first. Build a set from an ID-keyed dictionary when the source is not already in set order.

`LegPostureByPositionMapDictImpl` never holds an empty map.

## Side Effects

- `MaydayLeg.SetPosture` and everything built on it (`SetTipPositionTo`,
  `MoveTipPositionBy`, `MoveTipPositionTo`, and every `MaydayStructure`
  command) call `Joint.SetAngleGoal`. With hardware joints this moves the
  robot.
- `MaydayLegFactory.CreateLeg` calls `JointFactory.New` three times and
  connects the links it creates. What `New` does to hardware is the factory's
  contract (see [Dynamixel](../Dynamixel/README.md)).
- `MaydayStructureFactory.CreateDefault` attaches the leg links to the thorax
  link.
- `LegPostureByPositionMapDictImpl.GetFor` writes a line to the console when a
  cell has no posture.
- `DefaultMaydayStructure.GetCurrentLean` (and so `MoveThoraxTo`) asserts with
  `Debug.Assert` that ground clearance is above 0 and below 0.5 m.
- `LegPostureByPositionMapDictImpl.BuildNew` touches no hardware: it drives an
  echo leg in memory.

## Architectural Constraints

The direct Dynamixel project reference is a [documented exception](../knowledge/findings/mayday-domain-hardware-reference.md) to the intended direction.

## Planned namespace layout

An idea for where the root types should go. Nothing below is done or
scheduled; today all of these types sit in the root namespace.

The principle:

- **Root** holds few types: only those that are general to the whole project.
  They depend on most sub-namespaces, and no sub-namespace depends on them.
- **Base** is the opposite end: general types that depend on nothing else in
  the project, while almost every other type depends on them.
- **Internal** holds the types that code outside the project must not use,
  to keep the API surface small. Its types should also be `internal`. Other
  namespaces inside the project do not use them either; that is the
  repository's namespace rule, and it is stricter than the accessibility. The
  namespace is recent and not yet used to the full.
- Everything else lives in a sub-namespace for its topic.

Applied to the current types:

| Namespace | Types | Why |
|---|---|---|
| Base | `MaydayLegId`, `SidePosition`, `LegProperty<T>`, `MaydayStructureSet<T>` and its extensions, `MaydayLegPosture`, `MaydayStructurePosture` | The project's vocabulary: which leg, one value per leg, a posture. They depend only on each other and are used throughout. |
| [Components](./Components/README.md) | `MaydayStructure`, `DefaultMaydayStructure`, `MaydayLeg`, `JointLimits`, `MaydayLegFactory`, `EchoJointFactory`, the `LegPostureByPositionMap` types | Focused on the physical components. |
| Root | `MaydayStructureFactory`, or nothing | The closest fit: no sub-namespace uses it. It does not reach `MotionPlanning`, so it is not a true top. No type here ties components and motion planning together; `Robots` and `Main` do that. |

What stands in the way:

- `MaydayLegId.Value()` reads `MaydayLeg.JointCount`, so the ID depends on the
  leg. A base namespace would need that constant.
- Components and `MotionPlanning` depend on each other. `MaydayStructure`
  takes a `Motion`, and `DefaultMaydayStructure` calls `Stance`; the planners
  in `MotionPlanning` take a `MaydayStructure`. Moving types does not remove
  this cycle: `Stance` also needs `Components.Thorax`.

## Common Pitfalls

Do not infer physical orientation or safe angles from numeric IDs alone; test using echo joints first. See [posture lookups that silently keep the current posture](../knowledge/pitfalls/posture-map-silent-fallback.md).

- `LegPostureByPositionMapDictImpl.GetFor` does not throw for an unreachable
  tip position, despite the interface's documented exception: a cell without
  postures yields the current posture.
- `JointLimits.Defaults` is a mutable static field, and its values are marked
  in code as ballpark estimates. Nothing in this project checks a posture
  against them; only `ApplyForJointAngleRanges` reads them.
- `DefaultMaydayStructure` takes gravity as straight down the thorax Z axis and
  the thorax rotation as `Q.Unit`; there is no accelerometer input.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDomain|FullyQualifiedName~Test.Unit.Components.ThoraxTests"`
  (hardware-free; echo joints and Moq mocks).
- `Test.Integration.MaydayDomain` also holds `DefaultMaydayStructureTests` and
  `MaydayLegInverseKinematicsTests`: hardware-free, echo joints with the
  posture map the robot uses.
- `Test.Integration.MaydayDomain.MaydayStructureManualTests` is a
  `ManualRobotFact`: run it by hand only, after the operator confirms the
  robot, port and operating conditions.

## Related ADRs

[Stable abstractions](../docs/adr/0001-stable-robot-abstractions.md), [hardware boundary](../docs/adr/0002-hardware-adapter-boundary.md), [right-leg coxa direction and mirrored lookup](../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md).

## Related Findings

[Hardware reference](../knowledge/findings/mayday-domain-hardware-reference.md), [Scheduler construction](../knowledge/findings/scheduler-constructed-inside-domain-classes.md), [Posture map accuracy](../knowledge/findings/posture-map-accuracy.md), [Right coxa direction mismatch](../knowledge/findings/right-coxa-direction-mismatch.md)

## Related Root Causes

[Interpolation baseline drift](../knowledge/root-causes/interpolation-baseline-drift.md), [Empty leg posture map](../knowledge/root-causes/empty-leg-posture-map.md), [Rear femur hits the body](../knowledge/root-causes/rear-femur-hits-body.md), [Tip position test used the neutral map](../knowledge/root-causes/tip-position-test-used-neutral-map.md)

## Related Lessons Learned

[Goal versus measured state](../knowledge/lessons-learned/goal-versus-measured-state.md), [Predict on echo before hardware](../knowledge/lessons-learned/predict-on-echo-before-hardware.md).

## Related Failed Attempts

[Measured-angle interpolation](../knowledge/failed-attempts/measured-angle-interpolation.md).
