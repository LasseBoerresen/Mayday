# Right legs reverse the coxa and look up the left-leg map mirrored in Y

## Status

Accepted

## Context

Mayday's right legs are the mirror image of its left legs. The model has always given right coxas `RotationDirection.Reverse` (`MaydayLegFactory`), but nothing made the servos or the posture lookup agree with that:

- The posture map is built from one left leg (`LegPostureByPositionMapDictImpl.CreateEchoLeg`). With a reversed coxa, a posture from it puts a right tip at the mirror in Y, in the leg's own frame, of the target. On the echo model, "all tips 5 cm forward" moved the right tips about 5 cm backward or sideways, and the lean test measured 0.7 cm instead of 3 cm.
- `Driver.SetRotationDirection` could never set Drive Mode bit 0, so the servos never ran reversed. A read-only check on the robot (MAY-75) found Drive Mode 0 on all 18 servos, right coxas included. The robot moved its right tips the right way only because its unreversed coxas cancelled the map's mirroring.

So the echo model and the robot disagreed, and the echo model could not predict right-leg motion ([finding](../../knowledge/findings/right-coxa-direction-mismatch.md)).

## Decision

- Right coxas turn in reverse, in the model and on the servos. The model keeps `Reverse`; `Driver.Initialize` now writes it to Drive Mode bit 0 (PR #49).
- The posture map stays a left-leg map. `MaydayLegFactory` gives each leg whose coxa is reversed a `MirroredInYLegPostureByPositionMap`, which asks the left-leg map for the target mirrored in Y. The current posture passes through unmirrored, because the same joint values place a mirrored leg's tip at the mirror of a left leg's tip.
- The coxa direction is decided in one place (`MaydayLegFactory.CoxaRotationDirectionFor`), which both the joint and the lookup use.

## Alternatives Considered

- **Model right coxas as `Forward`, matching the servos as they were.** No servo change and no new lookup code, and the robot's past behavior stays. Rejected by the owner: a joint angle on a right leg would then mean the opposite physical turn to the same angle on a left leg, against the mirror symmetry the rest of the model assumes.
- **A separate map per side.** Exact for any asymmetry, but doubles the map file and its build time, and the two maps can drift apart.
- **Mirror inside `MaydayLeg`.** The leg would need to know its side, which today only the factory knows.

## Why

Mirroring the lookup is a few lines in the composition of a leg, keeps one map, and makes the echo model, the servos and the map agree on one convention: a joint angle means the mirrored physical turn on the other side. The cost is a one-time change on the robot, where the right coxas turn the other way from the next start.

## Consequences

- Benefits: the echo model predicts right-leg motion. Tip moves and `GetCurrentLean` are correct on both sides (`GivenSittingTall_WhenMoveAllTipsForward5cm_ThenTipMoves5cmForward`, the 3 cm lean test). `RotationDirectionOnRobotTests` checks the servos against the model.
- Costs: the right coxas change direction on the robot after PR #49. The first move after that should be predicted on echo and made with the legs clear or the robot lifted.
- Limitations: the mirror assumes right and left legs are exact mirror images in Y of their own frames (same links, no Y offsets beyond the coxa). An asymmetric leg would need its own map.

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [Dynamixel](../../Dynamixel/README.md)

## Related ADRs

[0002](0002-hardware-adapter-boundary.md)

## Related Findings

[Right coxa direction mismatch](../../knowledge/findings/right-coxa-direction-mismatch.md), [Posture map accuracy](../../knowledge/findings/posture-map-accuracy.md)

## Related Root Causes

None recorded.
