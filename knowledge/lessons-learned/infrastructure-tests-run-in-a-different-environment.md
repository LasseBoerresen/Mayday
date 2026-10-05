# Infrastructure tests must pass inside the thing they test

## Situation

The Pester tests for `scripts/gate.ps1` and `scripts/claude-stop-gate.ps1` passed 15 of 15 when run directly. Once the gate ran them as one of its own steps, one failed: the Stop hook test saw a `RemoteException` instead of the hook's stderr text.

## Observation

`gate.ps1` sets `$ErrorActionPreference = 'Stop'`, and Pester runs in a child scope that inherits it. The test helper captured a child process with `2>&1`, and under `Stop` Windows PowerShell 5.1 turns each stderr line of a native command into a terminating error. The tests themselves were right; the environment they ran in differed from the one they were written in. Only running them the way they will really run exposed it.

## Recommendation

Run tests of infrastructure code inside the real pipeline that will run them (here the gate) before calling them done, not only standalone. Make a test helper set the preferences it depends on (`$ErrorActionPreference`) instead of inheriting them. More generally, ambient state such as preference variables, environment variables and working directory is an input; pin it in the test.

## Related Components

[scripts](../../scripts/README.md)

## Related Tasks

[ADR 0007](../../docs/adr/0007-infrastructure-code-is-tested-like-product-code.md)
