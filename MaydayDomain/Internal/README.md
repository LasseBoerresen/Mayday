# MaydayDomain internals

## Overview

The types of `MaydayDomain` that code outside the project must not use
(namespace `MaydayDomain.Internal`). Grouping them in one namespace keeps the
project's API surface small and makes explicit which types are internal.

Inside the project the repository's namespace rule still applies: another
namespace, such as `MaydayDomain.MotionPlanning`, depends only on the root
types of `MaydayDomain`, not on the types here. That rule is stricter than
`internal` accessibility, which only closes the project boundary.

The namespace is recent and holds one type so far.

## Interface

### `MirroredInYLegPostureByPositionMap`

Lets a leg use a posture map that was built for its mirror image. It wraps a
`LegPostureByPositionMap` and is one itself.
File: `MirroredInYLegPostureByPositionMap.cs`.

| Member | What it does |
|---|---|
| Constructor `(map)` | Wraps the map built for the other leg. |
| `GetFor(tipPosition, currentPosture)` | Returns the wrapped map's answer for `tipPosition` with its Y negated. Passes `currentPosture` on unchanged. |

`MaydayLegFactory` is the only user in this project: it wraps the left-leg map
in this type for legs whose coxa turns in reverse (right legs). Taking the
left leg as the reference side is a guide for now, not a fixed rule.

## Invariants

- `currentPosture` is not mirrored: the same joint values place the mirrored
  leg's tip at the mirror of where they place the original leg's tip, so
  closeness between postures is unchanged
  ([ADR 0013](../../docs/adr/0013-right-legs-reverse-coxa-and-mirrored-posture-lookup.md)).
- Only `Y` is negated; `X` and `Z` of the tip position pass through.

Types in an `Internal` namespace should also have `internal` accessibility.
`MirroredInYLegPostureByPositionMap` is still `public`, so today only the
namespace marks it as internal. Planned: make it `internal`.

## Open questions

- If the `LegPostureByPositionMap` types move to `Components` (see the
  [planned namespace layout](../README.md#planned-namespace-layout)), whether
  this type follows into a `Components.Internal` or stays in a project-level
  `Internal` is undecided.
- More types of this project, and of other projects, may be organized this
  way over time.

## Side Effects

- None of its own. A lookup has the side effects and exceptions of the wrapped
  map's `GetFor`.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDomain.MirroredInYLegPostureByPositionMapTests"`
