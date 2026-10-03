# Empty leg posture map rejected by its own callers

## Symptoms

40 of 46 failing tests threw `LegPostureByPositionMapDictImpl.EmptyMapException`: 34 in `MaydayLegTests`, four in `DefaultMaydayStructureTests`, one in `MaydayLegInverseKinematicsTests` and one in `MaydayDayRobotFactoryTests`. `MaydayStructureFactory.CreateEcho()`, the hardware-free structure, could never be built.

## Investigation

- The `Map` setter in `MaydayDomain/LegPostureByPositionMapDictImpl.cs` throws `EmptyMapException` when the map is empty. `LegPostureByPositionMapFileRepoTests` relies on that check.
- `LegPostureByPositionMapDictImpl.CreateEmpty()` built an empty map, so it could never succeed. It was called from the test object mother, from seven test sites, and from production `MaydayLegFactory.NewEchoLegFactory()`.
- Fixing the object mother alone released one test (the robot-factory test). Replacing the seven test sites released 35 more, leaving ten; three of those structure tests still failed through the production echo factory.
- Releasing the tests from quarantine exposed assertion mismatches that the exception had hidden: the leg inverse-kinematics test, and `GivenStructureWithStandingPostureAndTipsMovedBackward1cm...Is1cmForward` (lean X is 0, expected 0.010). Their causes are not diagnosed.
- When the validation or `CreateEmpty` was introduced was not determined.

## Root Cause

Validation rejects empty maps, while a factory method and its callers kept building one. Production code and tests disagreed about what a valid map is, and the echo path in production was broken.

## Resolution

- `5db9c38`: the test object mother and the test call sites use a non-empty map; 36 tests left quarantine.
- `abf6a02`: `CreateEmpty()` was replaced by `CreateNeutral()`, a map with one origin-to-neutral mapping, and `NewEchoLegFactory` uses it. `CreateEcho()` now succeeds.
- Relaxing the validation was rejected: it would weaken the check that the map loader relies on.

## Prevention

A test suite that is red hides real defects behind one cheap, shared cause, so fix shared setup before judging individual failures. Keep hardware-free factories exercised by a test.

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [Test](../../Test/README.md)

## Related Tasks

[PR gating and test baseline](../task-summaries/pr-gating-and-test-baseline.md)

## Related Decisions

[ADR 0004](../../docs/adr/0004-quarantine-failing-tests-with-a-trait.md)
