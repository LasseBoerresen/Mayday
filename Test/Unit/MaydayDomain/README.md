# MaydayDomain unit tests

## Overview

Unit tests for the root types of [MaydayDomain](../../../MaydayDomain/README.md)
and its `Internal` namespace (namespace `Test.Unit.MaydayDomain`): the
structure, legs, leg factory and posture maps. Legs run on echo joints, fakes
or Moq mocks, never on hardware.

## Interface

Subdirectories:

| Directory | What it tests |
|---|---|
| [Components](./Components/README.md) | `Thorax.Origin`. |
| [MotionPlanning](./MotionPlanning/README.md) | `Stance` footprint and tip geometry. |

### `DefaultMaydayStructureTests`

Checks how `DefaultMaydayStructure` is assembled and passes postures and
stances to its six legs.
File: `DefaultMaydayStructureTests.cs`.

| Test | What it checks |
|---|---|
| `GivenSixUniqueLegs_WhenCreateMaydayRobot_ThenSucceeds` | A `DefaultMaydayStructure` can be built from a thorax link and six `MaydayLeg`s. |
| `GivenMaydayRobotWithMockLegs_WhenSetPosture_ThenCallsSetPostureOnAllLegs` | `SetPostureForAllLegs` calls `MaydayLeg.SetPosture` once on each of six mocked legs with the same timed posture. |
| `GivenMaydayStructure_WhenGetTransformsOfCoxaMotors_ThenReturnsCorrectTransformsInThoraxFrame` | On `MaydayStructureFactory.CreateEcho()`, `GetTransformsOf(LinkName.CoxaMotor)` equals `Thorax.TransformFor` for each leg. |
| `GivenStructureWithStandingPosture__WhenGetCurrentLean__ThenIsZero` | After `MaydayLegPosture.StandingWide` on all legs, `GetCurrentLean()` has zero X and Y. Z is not checked. |
| `GivenStandingStillMotion_WhenSetStance_ThenEachLegIsGivenItsNominalFootprintInItsOwnFrame` | `SetStance(Motion.StandingStill)` asks the posture map, per leg, for `Stance.GetTipPositionInThoraxFrameFor` viewed from that leg (within 0.1 mm). |
| `GivenStanceSetTwice_WhenSetStance_ThenBothRoundsRequestTheSameTipPositions` | Setting the same stance twice requests the same tip positions both times. |

| Helper | What it does |
|---|---|
| `RecordingLegPostureByPositionMap` (nested, private) | A `LegPostureByPositionMap` that records every requested tip position and returns the current posture. Lets the stance tests check geometry without a populated map. |

### `LegPostureByPositionMapDictImplTests`

Checks the neutral posture map and that the echo structure can be built.
File: `LegPostureByPositionMapDictImplTests.cs`.

| Test | What it checks |
|---|---|
| `GivenNeutralMap_WhenGetForOrigin_ThenReturnsNeutralPosture` | `LegPostureByPositionMapDictImpl.CreateNeutral()` returns `MaydayLegPosture.Neutral` for `Xyz.Zero`. |
| `GivenNeutralMap_WhenGetForUnmappedPosition_ThenFallsBackToCurrentPosture` | For an unmapped position the map returns the current posture. |
| `GivenEchoLegFactory_WhenCreateEchoStructure_ThenSucceeds` | `MaydayStructureFactory.CreateEcho()` does not throw. |

### `MaydayLegTests`

Checks `MaydayLeg` postures and link transforms, and the joint ids
`MaydayLegFactory` assigns.
File: `MaydayLegTests.cs`.

| Test | What it checks |
|---|---|
| `GivenLegWithFakeJointsOfAngle_WhenGetPosture_ThenReturnPostureWithThose` | Theory, 3 rows: `GetPosture()` returns the angles of the leg's `FakeJoint`s. |
| `GivenLeg_WhenSetPosture_ThenSetsJointsWithAngleGoals` | Theory, 3 rows: `SetPosture` calls `Joint.SetAngleGoal` once per joint with the posture's goal angles. |
| `GivenLegIdLeftFront_WhenCreateLegWithId_ThenCallsJointFactoryWithIds1And2And3` | `MaydayLegFactory.CreateLeg(LeftFront)` asks the `JointFactory` for joint ids 1, 2 and 3. |
| `GivenLegIdRightBack_WhenCreateLegWithId_ThenCallsJointFactoryWithIds16And17And18` | `CreateLeg(RightBack)` asks for joint ids 16, 17 and 18. |
| `GivenMockJointFactory_WhenCreateAll_ThenReturns6Legs` | `CreateAll()` returns six legs. |
| `GivenLegWithJointsAtZero_WhenGetLinkTransform_ThenReturnsExpected` | Theory, 7 links: the left center echo leg's link transforms at zero joints, data from `TestObjectFactory`. |
| `GivenLegWithJointsAtZeroButFirstAt0_25_WhenGetLinkTransform_ThenReturnsExpected` | Theory, 7 links: transforms with the coxa at 0.25 revolutions. |
| `GivenStraightLeg__WhenGetLinkTransform__ThenReturnsExpected` | Theory, 7 links: transforms in `MaydayLegPosture.Straight`. |
| `GivenLegWithJointsAtZeroButFemurAt0_0625_WhenGetLinkTransform_ThenReturnsExpected` | Theory, 4 links (coxa motor to femur): transforms with the femur at 0.0625 revolutions. The later links are commented out. |

| Helper | What it does |
|---|---|
| `CreateEchoMaydayLegFactoryWithJointsAt(jointState)` (internal static) | A `MaydayLegFactory` of `EchoJointFactory` joints and the neutral map. Ignores `jointState`. |

### `MirroredInYLegPostureByPositionMapTests`

Checks that the mirrored map looks up the mirror image of a tip position.
File: `MirroredInYLegPostureByPositionMapTests.cs`.

| Test | What it checks |
|---|---|
| `GivenMap_WhenGetForTipPosition_ThenReturnsMapsPostureForPositionMirroredInY` | `MirroredInYLegPostureByPositionMap.GetFor` asks the wrapped (mocked) map for the position with Y negated and the same current posture, and returns its answer. |

## Invariants

- Apart from the two stance tests, which use `RecordingLegPostureByPositionMap`,
  legs built here use `Test.Integration.MaydayDomain.Base.TestObjectMother.LegPostureByPositionMap`,
  which is `LegPostureByPositionMapDictImpl.CreateNeutral()`: one mapping at the
  origin, because an empty map throws `EmptyMapException`. Other positions fall
  back to the current posture, so these tests must not rely on a posture looked
  up away from the origin.
- `MaydayStructureFactory.CreateEcho()` also uses the neutral map
  (`MaydayLegFactory.NewEchoLegFactory`). The default map from
  `DefaultLegPostureByPositionMap.json` is not loaded here.
- Hardware-free: joints come from `EchoJointFactory`, `FakeJoint`
  (`Test.Unit.RobotDomain.Structures`), `TestObjectFactory.CreateMockJoint()` or
  `Mock<JointFactory>`. No physical, manual or quarantined tests.
- Postures are passed as `Timed<…>.Passed(…)`, so no clock is involved.
- Transforms are compared with `TestObjectFactory.AssertTransformEqual`
  (0.01 m, 0.01 revolution tolerance) unless a test passes its own precision.
- `MaydayLegTests` theories use positional `TheoryData<string, …>` rows with a
  string test id, not the nested `TestInput` record the test guidance asks for.

## Side Effects

- Console: a lookup of an unmapped cell in `LegPostureByPositionMapDictImpl`
  writes `No leg posture for cell position …, using current` to standard
  output (for example in
  `GivenNeutralMap_WhenGetForUnmappedPosition_ThenFallsBackToCurrentPosture`).
- `MaydayLegTests` writes each theory's test id to `ITestOutputHelper`.
- No files, hardware or user secrets.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDomain"`
  (includes the `Components` and `MotionPlanning` subdirectories).
