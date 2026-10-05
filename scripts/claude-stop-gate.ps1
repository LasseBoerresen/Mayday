<#
.SYNOPSIS
    Claude Code Stop hook: blocks Claude from finishing a turn while the local gate fails.

.DESCRIPTION
    Runs scripts/gate.ps1 in Debug. When the working tree has not changed since the last passing run this costs
    almost nothing, so chat-only turns are not slowed. On failure it exits 2 and writes the tail of the gate
    output to stderr, which Claude Code feeds back to Claude so it fixes the failure before stopping.
#>
$ErrorActionPreference = 'Stop'
[void][Console]::In.ReadToEnd() # hook input JSON; not needed, but drain it

$output = & (Join-Path $PSScriptRoot 'gate.ps1') -Configuration Debug 2>&1 | Out-String
if ($LASTEXITCODE -eq 0) { exit 0 }

$tail = ($output -split "`r?`n" | Select-Object -Last 60) -join "`n"
[Console]::Error.WriteLine("Local gate failed. Fix this before finishing:`n$tail")
exit 2
