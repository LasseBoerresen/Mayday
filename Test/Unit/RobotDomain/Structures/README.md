# RobotDomain structures test helpers

## Overview

Test doubles for the types in `RobotDomain.Structures`
([production README](../../../../RobotDomain/Structures/README.md)). The
folder holds no tests: it holds `FakeJoint`, which other unit tests use to
build structures with joints at fixed angles.

## Interface

### `FakeJoint`

A `Joint` that always reports the angle it was built with and cannot be moved.
File: `FakeJoint.cs`.

| Member | What it does |
|---|---|
| Constructor `(angle)` | Builds a joint with `Transform.Zero`, `RotationDirection.Forward`, `AttachmentOrder.LinkFirst`, a new `ComponentId`, between new `LinkName.Base` and `LinkName.Thorax` links. |
| `State` | Returns `JointState.Zero` with `Angle` set to the constructor's angle. |
| `SetAngleGoal(goal)` | Throws `NotImplementedException`. |
| `Transform` (protected) | Always `Transform.Zero`. |

User: `Test.Unit.MaydayDomain.MaydayLegTests` builds a `MaydayLeg` from
`FakeJoint`s to check that `GetPosture` reads the joints' angles.

## Invariants

- The state is fixed: only `Angle` differs from `JointState.Zero`, so goals,
  speed, load and temperature are those of `JointState.Zero`.
- Use it only where the code under test reads joint state. Any call to
  `SetAngleGoal` fails the test; use `EchoJoint` when goals must be accepted.

## Side Effects

- None.

## Verification

- No tests of its own. Run its user:
  `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDomain.MaydayLegTests"`
