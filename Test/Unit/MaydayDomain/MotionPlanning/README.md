# MaydayDomain motion planning unit tests

## Overview

Unit tests for `Stance`, the stance geometry of
[MaydayDomain.MotionPlanning](../../../../MaydayDomain/MotionPlanning/README.md)
(namespace `Test.Unit.MaydayDomain.MotionPlanning`). They call the static
`Stance` methods directly: pure geometry, no legs, joints or posture maps.

## Interface

### `StanceTests`

Checks where `Stance` places each leg's footprint on the ground and each tip
in the thorax frame, for a given `Motion`.
File: `StanceTests.cs`.

Most tests are theories that run once per leg (`DataForEachLeg`, all six
`MaydayLegId` values).

| Test | What it checks |
|---|---|
| `GivenStanceRadius_WhenGetFootprint_ThenIsOnGroundPlane` | `Stance.GetFootprintInGroundFrameFor` returns a footprint with `Z` zero. |
| `GivenStanceRadius_WhenGetFootprint_ThenIsThatRadiusFromThoraxOrigin` | The footprint is the stance radius from the thorax origin. |
| `GivenStanceRadius_WhenGetFootprint_ThenLiesAtItsOwnLegsAzimuthOnTheStanceCircle` | Each footprint lies at its own leg's azimuth: center legs on the Y axis, corner legs on the diagonals. |
| `GivenMountMovedFurtherFromOrigin_WhenGetFootprint_ThenStanceRadiusIsUnchanged` | The footprint stays the stance radius from the origin, although the leg mount (`Thorax.TransformFor`) is offset from it. |
| `GivenStanceRadius_WhenGetFootprints_ThenAllLegsAreEquallyFarFromThoraxOrigin` | All legs stand equally far from the origin, so the stance is a circle, not an irregular hexagon. |
| `GivenStandingStill_WhenGetTipPositions_ThenAllTipsAreGroundClearanceBelowThorax` | `Stance.GetTipPositionsInThoraxFrameFor` puts every tip the ground clearance below the thorax. |
| `GivenStandingStill_WhenGetTipPositions_ThenTipsAreSymmetricAroundThorax` | The mean of the six tips lies straight below the thorax origin (single `Fact`). |
| `GivenLeanForward1cm_WhenGetTipPositions_ThenAllTipsMoveBackward1cm` | A lean 1 cm forward moves every tip 1 cm back in the thorax frame. |
| `GivenLeanUp1cm_WhenGetTipPositions_ThenAllTipsMoveDown1cm` | A lean 1 cm up moves every tip 1 cm down. |
| `GivenLargerStanceRadius_WhenGetTipPositions_ThenTipsAreThatRadiusFromThoraxOrigin` | A 3 cm wider `Motion.StanceRadius` puts each tip, projected on XY, that radius from the origin. |
| `GivenPositiveRollLean_WhenGetTipPositions_ThenLeftTipIsFurtherBelowThoraxThanRightTip` | A positive roll lean puts the left center tip lower than the right center tip (single `Fact`). |
| `GivenSameMotion_WhenGetTipPositionsTwice_ThenResultIsIdentical` | Two calls with the same `Motion` return equal results (single `Fact`). |

Test data records nested in the class:

| Record | What it holds |
|---|---|
| `LegInput(Id, LegId)` | One leg for the per-leg theories. |
| `FootprintInput(Id, LegId, ExpectedFootprint)` | One leg and its expected footprint for the azimuth theory. |

## Invariants

- The tests build their own `Motion` (`MotionWith`, `StandingStill`) from a
  stance radius of 0.15 m and a ground clearance of 0.1 m. They do not use the
  production `Motion.StandingStill`.
- The expected footprints of the azimuth test come from the leg layout, not
  from `Thorax.TransformFor`, so an error in the mount geometry cannot make the
  test agree with the same error in `Stance` (comment in the data method).
- Tolerance is 0.1 mm (`Precision`), passed to the `TestObjectFactory` assert
  helpers instead of their 1 cm default, since this is pure geometry rather
  than an actuated pose.
- The mount-offset and equal-distance tests guard a real failure: the robot
  stood at 22 cm when 15 cm was asked for, because the radius was measured
  from the mount (comment in
  `GivenMountMovedFurtherFromOrigin_WhenGetFootprint_ThenStanceRadiusIsUnchanged`).

## Side Effects

- None.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDomain.MotionPlanning"`
