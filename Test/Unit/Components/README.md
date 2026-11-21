# Components unit tests

## Overview

Unit tests of how components attach and how a position is viewed from a leg
(namespace `Test.Unit.Components`). They cover `Link` and `Attachment` of
[RobotDomain.Structures](../../../RobotDomain/Structures/README.md) and the
`ViewedFrom` extension of `Thorax` in
[MaydayDomain.Components](../../../MaydayDomain/Components/README.md). The
directory name does not match a production project.

## Interface

### `ComponentAttachmentTests`

Checks the parent and child links of a base link and a thorax link joined by
one `Attachment`.
File: `ComponentAttachmentTests.cs`.

| Test | What it checks |
|---|---|
| `GivenBaseLinkAndAttachedLink_WhenGetChildrenOfBase_ThenReturnsAttachment` | `Attachment.NewBetween` adds the attachment to the base link's `Children`. |
| `GivenBaseLinkAndAttachedLink_WhenGetChildOfAttachment_ThenReturnsAttachment` | The attachment's `Child` is the attached link. |
| `GivenBaseLinkAndAttachedLink_WhenGetParentOfAttachment_ThenReturnsBaseLink` | The attachment's `Parent` is the base link. |
| `GivenBaseLinkAndAttachedLink_WhenGetParentOfLink_ThenReturnsAttachment` | The attached link's `Parent` is `Some` attachment. |
| `GivenBaseLinkAndAttachedLink_WhenGetParentOfBaseLink_ThenReturnsNone` | The base link's `Parent` is `None`. |
| `GivenBaseLinkAndAttachedLink_WhenGetTransformOfChildId_ThenReturnsTransformOfAttachment` | `Link.GetTransformOf(childId)` returns the attachment's transform (position within 1e-6 m, rotation by `IsRotationAlmostEqual`). |
| `GivenBaseLinkAndAttachedLink_WhenGetTransformOfNonChildId_ThenThrowsChildNotFoundException` | `GetTransformOf` an id that is no child throws `ChildNotFoundException`. |

### `ThoraxTests`

Checks that `Xyz.ViewedFrom(legId)` turns a position in the thorax frame into
the same position in that leg's frame.
File: `ThoraxTests.cs`. The repository's pattern for `TheoryData<TestInput>`
theories ([test guidance](../../CLAUDE.md)).

| Test | What it checks |
|---|---|
| `GivenXyzInThoraxFrame_WhenViewedFromLeg_ThenReturnsExpectedXyz` | Theory, 12 rows. Rows `0.x`: the thorax origin lies 0.1 m behind each center leg and about 0.113 m behind, 0.007 m to the side of, each corner leg. Rows `1.x`: each leg's own mount position (`Thorax.LeftFrontTransform` etc.) is `Xyz.Zero` in its frame. |

| Record | What it holds |
|---|---|
| `TestInput(Id, MaydayLegId, XyzInThoraxFrame, XyzInLegFrameExpected)` | One case; `Id` is a `TestId`. |

`Test.Unit.MaydayDomain.Components.ThoraxTests` has the same class name and
tests `Thorax.Origin` ([its README](../MaydayDomain/Components/README.md)).

## Invariants

- Each `ComponentAttachmentTests` test gets a fresh base link, thorax link and
  attachment from the constructor (xUnit creates one instance per test), so
  tests do not share link state.
- Expected leg-frame positions are written out as numbers or taken from the
  `Thorax` mount transforms; tolerance is the `TestObjectFactory.AssertXyzEqual`
  default of 0.01 m.

## Side Effects

- None.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.Components"`
