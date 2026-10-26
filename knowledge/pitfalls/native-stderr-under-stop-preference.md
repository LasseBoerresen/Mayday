# Pitfall

## Description

Calling a native command that writes to stderr (`dotnet test`, `git`) from Windows PowerShell 5.1 code that runs with `$ErrorActionPreference = 'Stop'` while its streams are redirected. Under redirection, Windows PowerShell turns each stderr line into an error record, and `Stop` makes the first one terminating. The script stops at a harmless line, though the command's exit code would have said it succeeded, or that the failure was expected.

## Why It Is Dangerous

The step that stops is often not the one that failed, so the report points at the wrong thing, and the same script passes when run another way. Occurrences:

- Pester tests for the Stop hook failed only when the gate ran them ([lesson](../lessons-learned/infrastructure-tests-run-in-a-different-environment.md)).
- 2026-10-24: `gate.ps1 -Physical`, called in-process with `*>&1`, aborted at the first quarantined test: `dotnet test` reports `[FAIL]` on stderr, though a failing quarantined test is expected. Fixed by running the test step with `Continue`.
- 2026-10-24: the quarantine ratchet and pre-push Pester tests failed inside the gate at their scratch commits, on git's `LF will be replaced by CRLF` warning. Fixed by giving the commit helpers `Continue`.

## Warning Signs

A `RemoteException` or `NativeCommandError` whose message is a warning or a test runner line; a script that passes standalone and fails inside the gate or a hook; `2>&1` or `*>&1` around a call into a script that sets `Stop`.

## Preferred Approach

Let exit codes and result files decide a native command's outcome. Run native commands whose stderr carries no verdict under `$ErrorActionPreference = 'Continue'`, set locally in the function or step that calls them, and check `$LASTEXITCODE`. Test infrastructure scripts the way they really run, including with redirected streams.

## Related Components

[scripts](../../scripts/README.md)
