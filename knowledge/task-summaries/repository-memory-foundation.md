# Repository memory foundation

## Problem

Architecture guidance and historical observations lived mainly in `README.md`, `AGENTS.md` (since renamed to `CLAUDE.md`), and `DevLog.md` without an indexed, repeatable way to preserve decisions and failure analysis.

## Solution

Added a knowledge index, navigable category directories, record templates, linked component documentation, and an agent workflow to consult and maintain them. Promoted one documented historical incident into linked root-cause, lesson, pitfall, and failed-attempt records while keeping the original log as evidence.

## Decisions Made

Record stable architecture and hardware boundaries as ADRs; explicitly identify the existing `MaydayDomain` dependency exception rather than asserting the intended direction is fully implemented.

## Tradeoffs

Documentation requires maintenance; index links and statuses must be updated together with new records. Existing code and historical source material remain unchanged.

## Follow-up Work

When changing the `MaydayDomain` hardware reference, trace consumers and tests before deciding whether to extract it. Add evidence-backed records as real changes and incidents occur.

## Related ADRs

[0001](../../docs/adr/0001-stable-robot-abstractions.md), [0002](../../docs/adr/0002-hardware-adapter-boundary.md)

## Related Findings

[MaydayDomain hardware reference](../findings/mayday-domain-hardware-reference.md)

## Related Root Causes

[Interpolation baseline drift](../root-causes/interpolation-baseline-drift.md) (historical incident documented, not fixed in this task)

## Related Lessons Learned

[Goal versus measured state](../lessons-learned/goal-versus-measured-state.md)
