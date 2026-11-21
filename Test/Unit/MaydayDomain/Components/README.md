# MaydayDomain components unit tests

## Overview

Unit tests for the `Thorax` record of
[MaydayDomain.Components](../../../../MaydayDomain/Components/README.md)
(namespace `Test.Unit.MaydayDomain.Components`). One test so far; the thorax
mount geometry and `ViewedFrom` are tested in
[Test/Unit/Components](../../Components/README.md).

## Interface

### `ThoraxTests`

Checks the thorax's own origin.
File: `ThoraxTests.cs`.

| Test | What it checks |
|---|---|
| `GivenThorax_WhenGetOrigin_ThenReturnsZeroTransform` | A new `Thorax` has `Origin` equal to `Transform.Zero`. |

A second test,
`GivenThorax_WhenGetLegOrigins_ThenReturnsTransformsOnCircleInCorrectOrder`,
is commented out and does not run. It refers to a `LegOrigins` member that
`Thorax` does not have.

A class with the same name, `Test.Unit.Components.ThoraxTests`, tests
`ViewedFrom`; filters by class name alone match both.

## Invariants

- Pure value check: no joints, maps, fixtures or time.

## Side Effects

- None.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDomain.Components"`
