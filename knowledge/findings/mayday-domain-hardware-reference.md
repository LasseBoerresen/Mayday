# Finding

## Description

`MaydayDomain` has a direct project reference to `Dynamixel`, despite the intended stable-core dependency direction. This is an observed exception, not a proposal to change dependencies in this documentation-only task.

## Why It Matters

Assuming the robot-specific domain is already hardware-independent could hide coupling during extraction, testing, or reuse.

## Evidence

`MaydayDomain/MaydayDomain.csproj` lists `Dynamixel/Dynamixel.csproj`. Compare the root README's dependency-inversion goal with the actual project references before planning changes.

## Recommendation

Trace the symbols and tests that require the reference before separating it; preserve existing functionality and document any subsequent decision in an ADR.

## Related Components

[MaydayDomain](../../MaydayDomain/README.md), [Dynamixel](../../Dynamixel/README.md)

## Related Decisions

[ADR 0001](../../docs/adr/0001-stable-robot-abstractions.md), [ADR 0002](../../docs/adr/0002-hardware-adapter-boundary.md)
