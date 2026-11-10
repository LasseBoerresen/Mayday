# Shared robot structures

## Responsibility

Model links, attachments, joints, wheel state, and composed structures.

## Interface

- **Links:** `Link`, a node of the kinematic tree with `Id` (`ComponentId`), `Name` (`LinkName`), `Parent` (`Option<Connection>`) and `Children` (`List<Connection>`), built with `Link.New(LinkName)` or the `Create<Name>` properties, each giving a new link (`Link.cs`).
  - `GetTransformOf(ComponentId childId)`: the pose of a descendant in this link's frame; throws `ChildNotFoundException` if it is not found.
  - `ConnectParent(Connection)`, `ConnectChild(Connection)`, `LookForTransformsTo(…)`: graph wiring and the search behind `GetTransformOf`.
- **Connections:** `Connection`, an abstract edge from `Parent` to `Child` link with `Id`, `Name` (`<Parent>To<Child>`) and a protected abstract `Transform` (`Connection.cs`). `Attachment.NewBetween(Link parent, Link child, Transform transform)` makes a fixed one (`Attachment.cs`).
- **Joints:** `Joint`, an abstract actuated connection with `State` (`JointState`) and `SetAngleGoal(Timed<Angle>)` (`Joint.cs`).
  - `EchoJoint`: a joint without hardware whose `State.Angle` is the target of the last goal it was given (`EchoJoint.cs`).
  - `JointFactory.New(parent, child, passiveTransform, JointId, RotationDirection, AttachmentOrder)`: the interface for creating joints; composition roots supply the implementation (`JointFactory.cs`).
- **Joint state:** `JointState(Angle, RotationalSpeed, LoadRatio Torque, Temperature, Timed<Angle> AngleGoal, Timed<Angle> AngleGoalPrevious)`, an immutable snapshot, with `Zero` and `InterpolateGoalAngleOneTimeStep(Func<Timed<Angle>, double>)` (`JointState.cs`). `Motor` is an interface exposing a `JointState State` (`Motor.cs`).
- **Structure:** `Structure.GetTransformFor(ComponentId)`, not implemented yet (`Structure.cs`).
- **Identity and values:** `ComponentId(Guid)` with `ComponentId.New`; `JointId(int) : Motion.ActuatorId` with `FromBase(ActuatorId)`; `WheelState(Speed)`.
- **Enums:** `AttachmentOrder { LinkLast = 1, LinkFirst = -1 }`, `RotationDirection { Forward = 1, Reverse = -1 }`, `Side { Left, Right }`, `LinkName` (the link names `Link.New` accepts).
- **Exceptions:** `ChildNotFoundException(Link, ComponentId)`; `ConnectionHasNoParentException`.

## Key Concepts

- **Kinematic tree:** links are nodes and connections are edges from a parent link to a child link. A connection's transform places the child in the parent's frame, so `GetTransformOf` composes the transforms on the path down to a descendant (`Transform` `+`, see [Geometry](../Geometry/README.md)).
- **Attachment versus joint:** an attachment's transform is fixed. A joint's transform combines a fixed passive transform with an active rotation about z by the joint's angle, so it changes as the joint moves.
- **Angle sign:** `RotationDirection` says which way the actuator turns for a positive angle, and `AttachmentOrder` says whether the moving link comes after or before the passive transform. Both flip the sign of the angle used in the transform.
- **Goal history:** a `JointState` keeps the current and the previous commanded goal. Interpolation runs from the previous goal to the current one, never from the measured angle. See the [measured-angle baseline pitfall](../../knowledge/pitfalls/interpolation-baseline.md).

## Dependencies

Part of `RobotDomain`. Uses:
- `RobotDomain.Geometry` for `Transform` and `Q`;
- `RobotDomain.Physics` for `LoadRatio`;
- `RobotDomain.Time` for `Timed`;
- `RobotDomain.Motion` for `ActuatorId`, which `JointId` extends (see Known Issues);
- `UnitsNet` for every physical quantity, and `LanguageExt` for `Option`.

## Dependency Rules

Keep robot-specific link, joint and wheel layouts in the robot projects that use this one.

## Invariants

`JointState.InterpolateGoalAngleOneTimeStep` starts at the previous commanded goal, not the measured angle.

- **Interpolation:** returns `AngleGoalPrevious.Target + (AngleGoal.Target − AngleGoalPrevious.Target) × factor`; the factor comes from the caller and is not clamped here.
- **Wiring:** building a `Connection` registers it on both links: `Parent.ConnectChild(this)`, then `Child.ConnectParent(this)` (`Connection.cs`).
- **Search:** `GetTransformOf` searches depth-first through `Children` in insertion order, and the first match wins. The chain starts at `Transform.Zero` and never changes the caller's list.
- **Angle sign:** the angle used in the transform is `State.Angle × RotationDirection × AttachmentOrder`, each ±1. The active rotation is `Q.FromRpy(0, 0, angle)`; `LinkLast` composes active then passive, `LinkFirst` passive then active (`Joint.cs`).
- **Echo joint:** `EchoJoint` starts with both goals at `Timed<Angle>.Passed(Angle.Zero)`, and `SetAngleGoal` moves the current goal to previous before storing the new one. It reports zero speed, zero load and 23 °C.
- **Mutability:** `ComponentId`, `JointId`, `JointState` and `WheelState` are immutable records; `Link.Children` is a public mutable list.
- **Validation:** constructors and factories do not validate their arguments.

## Known Issues

- `Structure.GetTransformFor` always throws `NotImplementedException`: [MAY-96: Need: No public RobotDomain member throws NotImplementedException](https://linear.app/mayday-robot/issue/MAY-96).
- `Motor`, `ConnectionHasNoParentException` and `Structure` are used nowhere: [MAY-105: Need: RobotDomain.Structures holds no unused types](https://linear.app/mayday-robot/issue/MAY-105).
- `LinkName` and the `Link.Create<Name>` properties name the links of one robot: [MAY-99: Need: RobotDomain code names no specific robot](https://linear.app/mayday-robot/issue/MAY-99).
- `JointId : ActuatorId` makes Structures depend on Motion, which depends on Structures: [MAY-98: Need: Motion and Structures depend in one direction only](https://linear.app/mayday-robot/issue/MAY-98).
- `JointState.Torque` holds a `LoadRatio`, not a torque: [MAY-104: Need: JointState names an actuator's load a load, not a torque](https://linear.app/mayday-robot/issue/MAY-104).
- The tree is not validated: a second parent silently replaces the first, and duplicate children and cycles are accepted. Because `Link.Children` is a public mutable list, any caller can rewire the tree.

## Side Effects

- Building a `Connection` (`Attachment.NewBetween`, any `Joint`) mutates both links: it appends to `Parent.Children` and sets `Child.Parent`.
- `EchoJoint.SetAngleGoal` mutates the joint's stored goals.
- `ComponentId.New`, and so `Link.New`, the `Link.Create<Name>` properties and `Attachment.NewBetween`, draws a random `Guid`.
- No hardware access, logging or console output; hardware joints come from a caller-supplied `JointFactory`.

## Verification

`dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.RobotDomain.Structures"`

## Architectural Constraints

Joint IDs, orientation, and graph ordering affect kinematics and physical motion.

## Common Pitfalls

[Measured-angle baseline](../../knowledge/pitfalls/interpolation-baseline.md).

## Related ADRs

[Stable abstractions](../../docs/adr/0001-stable-robot-abstractions.md), [C# structure as kinematic source](../../docs/adr/0016-the-csharp-structure-stays-the-kinematic-source.md).

## Related Findings

None recorded.

## Related Root Causes

[Interpolation drift](../../knowledge/root-causes/interpolation-baseline-drift.md).

## Related Lessons Learned

[Goal versus measured state](../../knowledge/lessons-learned/goal-versus-measured-state.md).

## Related Failed Attempts

[Measured-angle interpolation](../../knowledge/failed-attempts/measured-angle-interpolation.md).
