# Knowledge record templates

Copy the appropriate template into `docs/adr` or `knowledge/<category>` with a descriptive, stable filename. Replace prompts with evidence and link related records using relative paths. Do not create empty records for hypothetical incidents. Add the new record to its directory README and update `knowledge/INDEX.md` when it contains significant knowledge.

For recurring pitfalls and rejected approaches, use the formats described in `knowledge/pitfalls/README.md` and `knowledge/failed-attempts/README.md`.

Add diagrams where they clarify relationships or chronology: Mermaid dependency/flow or class diagrams for architectural records and component READMEs; sequence diagrams for interactions and failures. Keep the editable diagram source alongside the text (fenced Mermaid is preferred for inline GitHub rendering; PlantUML or another text format is acceptable with a viewable rendering or source link). Explain the diagram in prose, distinguish current behavior from intended design, and update it when the documented behavior changes. Link an existing maintained diagram rather than duplicating it.
