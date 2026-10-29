# The tip position test used the one-posture neutral map

## Symptoms

`MaydayLegInverseKinematicsTests.WhenSetReachableTipPositionTo_TheReachesThatPosition` was quarantined as "assertion mismatch on the reached tip position; previously hidden by EmptyMapException, cause not yet diagnosed" (MAY-78). For a target of (0.200, 0, 0) m, the left center echo leg's tip ended at (0.166, 0, −0.127) m.

## Investigation

- The leg came from `MaydayLegTests.CreateEchoMaydayLegFactoryWithJointsAt`, which builds its legs with `TestObjectMother.LegPostureByPositionMap`, i.e. `LegPostureByPositionMapDictImpl.CreateNeutral()`: one entry, `Xyz.Zero → MaydayLegPosture.Neutral`.
- The reached position (0.166, 0, −0.127) m is the tip at the Neutral posture, the same position an echo leg starts at.
- With the default map (`TestObjectMother.DefaultLegPostureByPositionMap`) and nothing else changed, the test passed within its 1 cm tolerance.
- With the default map but `SetTipPositionTo` not setting the posture, the test failed again, so it still discriminates.

## Root Cause

The neutral map exists so that tests which never look up postures can build legs ([empty leg posture map](empty-leg-posture-map.md)). This test does look up a posture, so every lookup landed on the only posture the map holds. The quarantine reason called it undiagnosed because the earlier `EmptyMapException` had hidden it.

## Resolution

PR #48: the test moved to `Integration/MaydayDomain` (it loads the map file), builds its leg with the new `TestObjectMother.CreateEchoLegWithDefaultMap`, and left quarantine with its expectation unchanged.

## Prevention

A test that moves a tip or looks up a posture uses `DefaultLegPostureByPositionMap` (or a map built for its targets), never the neutral map. The XML doc of `DefaultLegPostureByPositionMap` in `Test/Integration/MaydayDomain/Base/TestObjectMother.cs` says so.

## Related Components

[Test](../../Test/README.md), [MaydayDomain](../../MaydayDomain/README.md)

## Related Tasks

[Quarantine cleanup and right-leg mirroring](../task-summaries/quarantine-cleanup-and-right-leg-mirroring.md)

## Related Decisions

[ADR 0004](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md)
