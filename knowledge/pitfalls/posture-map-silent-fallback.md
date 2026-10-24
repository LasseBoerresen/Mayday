# Pitfall

## Description

Asking a leg for a tip position the posture map has no posture for. `LegPostureByPositionMapDictImpl.GetClosestFor` then returns the leg's current posture and only writes `No leg posture for cell position …, using current` to the console. The leg stays where it is and the caller is not told.

## Why It Is Dangerous

The motion looks partly right: some legs move and others silently do not. On the robot this was taken at first for a mechanical or mirroring problem. Tests that check only some legs, or check loosely, pass anyway.

## Warning Signs

- `No leg posture for cell position` in the console or test output.
- A leg that does not move at all while the others do.
- Targets that pull a tip in towards the body (rear legs moving forward, front legs moving backward) or far out (beyond about 20 cm from the mount).

Seen three times while building the manual tips-forward test (PR #45):

- On the robot: from `Sitting`, moving all tips 5 cm forward left both rear legs in place. The echo model showed the fallback for exactly those legs.
- On the echo model: from `SittingTall`, 6 cm forward left both front legs in place (a hole in the map), and 10 cm forward moved the front legs and the right rear only about half way.

## Preferred Approach

Before a physical run, do the same motion on an echo copy of the structure with the same map, and check that every leg moves ([predict on echo before hardware](../lessons-learned/predict-on-echo-before-hardware.md)). Choose start postures and move sizes the map covers for all legs. A lookup that reports an unreachable target instead of falling back is planned under MAY-76.

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [MaydayDataAccess](../../MaydayDataAccess/README.md), [Test](../../Test/README.md)
