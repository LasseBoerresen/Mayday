# RobotDomain geometry unit tests

## Overview

Unit tests of the value types in `RobotDomain.Geometry`
([production README](../../../../RobotDomain/Geometry/README.md)): the
quaternion `Q`, `Transform` and `Xyz`. Each test builds values directly and
checks one operation; no structure, joint or clock is involved.

## Interface

### `QTests`

Checks that quaternions built from roll-pitch-yaw or axis-angle compose and
rotate points as expected.
File: `QTests.cs`.

| Test | What it checks |
|---|---|
| `GivenRpy_WhenFromRpy_ThenReturnsExpectedQ` (theory, 17 rows) | `Q.FromRpy` gives the expected orientation for zero, whole-turn, half-, quarter-, sixth-, third- and three-quarter turns about each single axis (`IsOrientationAlmostEqual`, precision 0.001). |
| `GivenTwoRpy_WhenAdd_ThenReturnsExpectedQ` (theory) | `Q.FromRpy(a) + Q.FromRpy(b)` gives the expected orientation, including two half turns cancelling out. |
| `GivenThreeAxisAngleQs_WhenAdd_ThenEqualFromRpyQ` | `qZ + qY + qX` from `Q.FromAxisAngle` equals `Q.FromRpy(new(0.25, -0.125, 0.5))` (`IsRotationAlmostEqual`). |
| `Rotate_IdentityQuaternion_ReturnsSameXyz` | `Q.Unit.Rotate` leaves a point unchanged. |
| `Rotate_0_25Rev…`, `Rotate_0_5Rev…`, `Rotate_0_125RevAroundZAxis_RotatesXPointCorrectly` | `Q.Rotate` turns unit points about X, Y and Z by a quarter, half and eighth turn to the expected `Xyz`. |
| `Rotate_PointFurtherThanOneMeter_RotatesPointCorrectly` | Rotation also holds for a point 10 m from the origin. |
| `Rotate_NonUnitQuaternion_NormalizesBeforeRotation` | `new Q(0.5, 0, 0, 0)` rotates like the identity: `Rotate` normalizes first. |
| `Rotate_Neg1X_1Y_…`, `Rotate1X…`, `RotateNeg1X…`, `Rotate1Y…FromTibiaMotorToTibiaCorrectly` | The tibia-motor-to-tibia rotation `Q.FromRpy(new(0.25, -0.125, 0.5))` maps four points to the expected `Xyz` (within 1 mm). |

### `TransformTests`

Checks that transforms chain with `+` and interpolate halfway.
File: `TransformTests.cs`.

| Test | What it checks |
|---|---|
| `GivenTwoTransforms__WhenAdd__ThenReturnsExpectedTransform` (theory) | `a + b` adds translations, and a quarter yaw in `a` turns `b`'s 1 m X offset into a 1 m Y offset. |
| `GivenThreeTransforms__WhenAddAndAdd__ThenReturnsExpectedTransform` (theory) | `a + b + c` chains translations and rotations, including three quarter yaws. |
| `GivenNonZeroTransform__WhenCalculateHalfwayToZero__ThenShouldBeHalfway` | `Transform.HalfWayTo(Transform.Zero)` halves both the translation and the axis-angle rotation. |

### `XyzTests`

Checks `Xyz` construction.
File: `XyzTests.cs`.

| Test | What it checks |
|---|---|
| `GivenCoordinatesBeyondTheRobotsReach_WhenCreate_ThenKeepsTheCoordinates` (inline theory) | `new Xyz(x, y, z)` keeps large coordinates (10 m, -250 m, 1000 m) as meters, without clamping. |
| `GivenNaNCoordinate_WhenCreate_ThenThrowsArgumentException` | A `NaN` coordinate throws `ArgumentException`. |

The shared helpers come from `Test.Unit.TestObjectFactory`
(`AssertTransformEqual`, `TestMessage`), not from this folder.

## Invariants

- `Rpy(double, double, double)` takes revolutions, so `0.5` is a half turn
  and `0.25` a quarter turn in all theory data.
- Comparisons are approximate: quaternions within 0.001
  (`IsOrientationAlmostEqual`), points within `1e-6` m or `1e-3` m
  (`Xyz.IsAlmostEqual`), transforms within 0.01 m and 0.01 revolutions
  (`TestObjectFactory.AssertTransformEqual`).
- `Q.FromAxisAngle` needs a unit axis; `TransformTests` normalizes its axis
  with `Vector3.Normalize` for that reason.
- The theories use positional `TheoryData<string, …>` with a string test id,
  and the `Rotate_*` tests predate the `Given_When_Then` naming in
  [the test guidance](../../../CLAUDE.md).

## Side Effects

- None. The tests only write diagnostics through `ITestOutputHelper`.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.RobotDomain.Geometry"`
