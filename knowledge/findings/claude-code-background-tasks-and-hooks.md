# Claude Code background tasks and hooks

## Description

Measured on 2026-11-03 in the Claude desktop app (Code tab), on this repository's Windows machine:

- A `PostToolUse` hook on a shell tool fires when a background command **starts**, not when it ends.
- A `PostToolUse` hook's plain standard output does not reach the agent; it appears only in the transcript.
- A background task that finishes after the session's turn has ended wakes the session with a new turn, carrying the task's output. A 16-minute background task ran to the end and was not killed.
- `claude -p` runs when started from inside a Claude Code tool call, with `CLAUDECODE=1` set; nothing needs clearing.

## Why It Matters

Anything started by a `PostToolUse` hook after a background `git push` runs before the push has finished, so it sees the old remote state. A hook cannot tell the agent anything through plain output. A background task the session starts itself is a working way to have the session wait for slow work without polling.

## Evidence

- Hook timing: a background PowerShell command `"start $(Get-Date -Format HH:mm:ss)"; Start-Sleep 45; "end ..." # gh pr create` printed start 23:49:35 and end 23:50:20; the review hook it triggered created its log `review-20261103-234935-34436.log` at 23:49:35.
- Hook output: the hook's line "Started a separate Claude review …" did not appear in the tool result the agent received.
- Wake: a background `sleep 960` ran from 23:49:32 to 00:05:33, exited 0, and its completion started a new turn; a 45-second background task did the same after the turn had ended.
- Nested call: `'Reply with exactly: nested-ok' | claude -p --setting-sources user --no-session-persistence --output-format text` from a PowerShell tool call returned `nested-ok`, exit 0, in 8 s, with `CLAUDECODE=1`.

Not checked: whether `hookSpecificOutput.additionalContext` reaches the agent; behavior in the Claude Code CLI or headless sessions.

## Recommendation

Start follow-up work after a push from the session itself, as a background task, not from a `PostToolUse` hook. Do not rely on a hook's plain output to inform the agent.

## Related Components

[scripts](../../scripts/README.md)

## Related Decisions

[ADR 0014](../../docs/adr/0014-authoring-session-follows-the-review.md), [ADR 0010](../../docs/adr/0010-local-claude-review-of-every-pull-request.md)
