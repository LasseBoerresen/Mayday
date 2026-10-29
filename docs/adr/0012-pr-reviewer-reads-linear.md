# The PR reviewer reads Linear

## Status

Accepted. Amends the reviewer's tool boundary in [ADR 0010](0010-local-claude-review-of-every-pull-request.md); the rest of ADR 0010 stands.

## Context

CLAUDE.md asks that every Linear issue or project in messages, PR descriptions and comments be a link with its ID and title, taken from Linear rather than from memory. The review session of [ADR 0010](0010-local-claude-review-of-every-pull-request.md) posts PR comments, but its tools were limited to reading files and `gh pr diff|view`, `git diff|log|show`. Every comment that mentioned an issue could therefore only break the rule or guess a title (finding on PR #54).

## Decision

The review session may also use four read-only tools of the owner's claude.ai Linear connector: `mcp__claude_ai_Linear__get_issue`, `get_project`, `list_issues` and `list_projects`. These are named tool by tool. It gets no server-wide Linear rule, no wildcard and no tool that writes (`save_*`, `create_*`, `delete_*` and the like). Its prompt asks for Linear issues and projects as links with ID and title looked up there, falling back to the ID and URL when a lookup fails. A Pester test pins the Linear entries to exactly these four tools.

## Alternatives Considered

- Exempt automated review comments from the link rule: keeps the boundary, but reviews would keep citing bare IDs, the problem the rule exists to remove.
- Give the reviewer the whole Linear connector: simpler, but it could then write to Linear, which a reviewer never needs.

## Why

Reading an issue's title is all the rule needs, and the connector comes with the owner's claude.ai login, which the headless session uses. Which settings the session loads (`--setting-sources user`) does not decide whether the connector is there; only `--allowedTools` limits what it may call. Naming four read tools keeps the reviewer unable to change anything, as ADR 0010 intends for GitHub.

## Consequences

- Benefits: review comments can give Linear references as titled links.
- Costs: the reviewer now reaches an external service with the owner's Linear login; a failed lookup costs a fallback, not a failed review.
- Limitations: issue and project text read from Linear reaches the reviewer's context, so it could influence the comment; the tools only read, so nothing else is exposed. The connector's tool names are fixed in the script, so a renamed connector needs the list updated.

## Related Components

[scripts](../../scripts/README.md)

## Related ADRs

[0010](0010-local-claude-review-of-every-pull-request.md), [0007](0007-infrastructure-code-is-tested-like-product-code.md)

## Related Findings

None recorded.

## Related Root Causes

None recorded.
