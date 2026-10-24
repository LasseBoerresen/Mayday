# Finding

## Description

Tip placement through the leg posture map (`LegPostureByPositionMapDictImpl.GetFor`, with the map in `MaydayDataAccess/DefaultLegPostureByPositionMap.json`) is accurate to about 1 cm, not to the millimetres its interpolation suggests.

## Why It Matters

Every tip-position motion goes through this lookup: `MaydayLeg.MoveTipPositionTo`, `DefaultMaydayStructure.MoveTipsBy`/`MoveTipsTo`, `SetStance` and the `TrackingMaydayMotionPlanner` the robot uses to sway. A test that moves tips by about 1 cm and checks the result at 1 cm cannot tell a working lookup from one that does nothing: the lean test passed at 1 cm with a lean of 2 mm against 10 mm expected.

## Evidence

Measured on the echo model with the real map, during MAY-71:

- The map has 7365 cells of 1/64 m (1.56 cm), built with 1/64 revolution (5.6°) joint steps from one left leg, and holds 43,264 postures, about 6 per cell.
- `BuildNew` keys each posture by the cell its tip falls in. A stored posture puts the tip a median 7.6 mm from its cell center (90th percentile 10.2 mm, maximum 13.4 mm), but `GetFor` treats it as exactly at the center.
- `GetClosestFor` picks the stored posture nearest the current posture, not the one nearest the cell center.
- `GetFor` blends with a single neighbour cell, so the target's offset along the other two axes is dropped.
- Sweeping the left rear leg from −20 mm to +20 mm along X from its standing tip gave errors up to 11 mm, with a sawtooth at cell borders; a 0 mm target still moved the tip 3 mm.

## Recommendation

Treat map-based tip moves as accurate to about 1 cm: test moves that are several times larger than that, or tolerances well below the move. Analytical inverse kinematics is planned as an alternative that the map can be compared against (MAY-74).

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [MaydayDataAccess](../../MaydayDataAccess/README.md), [Test](../../Test/README.md)

## Related Decisions

None recorded. See also the [silent fallback pitfall](../pitfalls/posture-map-silent-fallback.md) and [empty leg posture map](../root-causes/empty-leg-posture-map.md).
