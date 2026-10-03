---
name: record-knowledge
description: Record repository knowledge (ADR, finding, root cause, lesson, pitfall, failed attempt, task summary) under docs/adr or knowledge/. Use after a decision is made or superseded, a non-obvious fact is discovered, a failure is diagnosed, an approach is abandoned, or significant work completes, or when the user asks to record or document what was learned.
---

# Record knowledge

Turn evidence from the current work into durable records the next session can find through [knowledge/INDEX.md](../../../knowledge/INDEX.md). A record states what was **observed**; a guess stays labelled as a hypothesis.

## 1. Gather evidence

List what this work established: decisions and their alternatives, facts confirmed in code or by running something, failures with a confirmed cause, approaches tried and rejected, and the commits involved. Read the existing records in the matching directories so each item either extends an existing record or is genuinely new.

**Done when:** every item has its evidence (file, commit, test output, measurement or user statement) and you know whether a record already covers it.

## 2. Classify

| Evidence | Record | Location | Format |
|---|---|---|---|
| A decision with alternatives and consequences | ADR | `docs/adr/NNNN-short-title.md` (next number) | `templates/adr-template.md` |
| A changed accepted decision | New ADR + old ADR marked `Superseded` with a link to its successor | `docs/adr/` | `templates/adr-template.md` |
| A non-obvious fact about the code, hardware or tools | Finding | `knowledge/findings/` | `templates/finding-template.md` |
| A failure whose cause is confirmed | Root cause | `knowledge/root-causes/` | `templates/root-cause-template.md` |
| A reusable insight | Lesson learned | `knowledge/lessons-learned/` | `templates/lesson-learned-template.md` |
| A mistake seen recurring, with evidence | Pitfall | `knowledge/pitfalls/` | headings in `knowledge/pitfalls/README.md` |
| An approach actually tried or evaluated, then rejected | Failed attempt | `knowledge/failed-attempts/` | headings in `knowledge/failed-attempts/README.md` |
| Significant completed work | Task summary | `knowledge/task-summaries/` | `templates/task-summary-template.md` |

An unconfirmed cause is a finding with the hypothesis stated as such, not a root cause. A risk with no observed occurrence gets no record. Follow [templates/README.md](../../../templates/README.md) for naming, linking and diagrams.

## 3. Propose and wait

Present the list to the user: for each record, its type, filename, a one-line summary, and whether it is new or updates an existing record. Also list the bookkeeping edits from step 5.

**Done when:** the user has approved, edited or dropped each item. Write only the approved records.

## 4. Write

Fill the template with the evidence from step 1, linking related records, ADRs, components and commits with relative paths. Remove template prompts and fill every section; write "None recorded" where a section truly has nothing.

## 5. Update every pointer

For each record written:
- add it to its directory `README.md` (the ADR table in `docs/adr/README.md`, the entries list elsewhere);
- add or update its entry in `knowledge/INDEX.md`, keeping the "recent" lists current;
- add it to the matching "Related …" section of each affected component `README.md`, replacing "None recorded".

**Done when:** every new record can be reached from the index, from its directory README and from each component it concerns, and every link resolves.

## 6. Commit

Commit the records as their own `docs:` commit, separate from code changes, using the `Problem:` / `Rationale:` / `Impact:` format in the root `CLAUDE.md`.
