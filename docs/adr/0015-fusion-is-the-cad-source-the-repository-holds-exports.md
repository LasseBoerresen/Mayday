# Fusion 360 is the CAD source; the repository holds exports

## Status

Accepted.

## Context

Mayday's mechanical design lives in Fusion 360. The electronics had no design document, and nothing in the repository tied the physical robot to the code that drives it. The design will change in steps (power distribution, battery, onboard computer, charging nest), and software such as collision checks and simulation will need its geometry.

## Decision

- Fusion 360 stays the place where the mechanical design is edited and versioned. Its `.f3d` files are not committed.
- The repository holds what software and readers consume, under [`hardware/mayday/`](../../hardware/mayday/README.md): the bill of materials, wiring diagrams in Mermaid, the power budget and a log of **hardware revisions** (see [CONTEXT.md](../../CONTEXT.md)).
- Geometry exported from Fusion (STEP, STL) goes in `hardware/mayday/exports/`, stored with Git LFS, and each export commit names the Fusion version it came from. Not set up yet: it starts with the first export.
- Each hardware revision gets a Fusion milestone and a git tag (`mayday-hw-<letter>`).
- Electronics are bought as modules, so there is no schematic or PCB source; KiCad is not adopted.

## Alternatives Considered

- `.f3d` archives in git LFS: a backup only; git can neither diff nor merge them, and Fusion already keeps their history.
- Code-CAD (build123d, CadQuery, OpenSCAD): text, diffable and parametric, a natural fit for a developer, but slow for Mayday's organic leg shapes and a rewrite of the existing model.
- Onshape: branching and merging built in, but a switch of tool for no current need.
- A separate hardware repository or a PDM/PLM system, as companies with mechanical teams use: the link between disciplines becomes part number and revision; overhead with one person.

## Why

Fusion is good at editing the design and already versions it; git is good at what text tools read. Keeping each where it is strong, and linking them by revision, lets one commit change an export, the code that uses it and the docs together.

## Consequences

- Benefits: hardware changes are reviewed and tagged like code; software can depend on exported geometry; no CAD tool change.
- Costs: exporting is a manual step, and the export can lag the Fusion design.
- Limitations: the history of the design itself stays in Fusion, outside git; an export is only as current as its last commit.

## Related Components

[hardware/mayday](../../hardware/mayday/README.md)

## Related ADRs

None recorded.

## Related Findings

None recorded.

## Related Root Causes

None recorded.
