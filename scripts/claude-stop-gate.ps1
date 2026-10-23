<#
.SYNOPSIS
    Claude Code Stop hook: blocks Claude from finishing a turn while the local gate fails.

.DESCRIPTION
    Runs scripts/gate.ps1 in Debug. When the working tree has not changed since the last passing run this costs
    almost nothing, so chat-only turns are not slowed. On failure it exits 2 and writes the tail of the gate
    output to stderr, which Claude Code feeds back to Claude so it fixes the failure before stopping.
#>
param([string] $GateScript)

$ErrorActionPreference = 'Stop'
if (-not $GateScript) { $GateScript = Join-Path $PSScriptRoot 'gate.ps1' }
[void][Console]::In.ReadToEnd() # hook input JSON; not needed, but drain it

# The gate runs in its own process, so every way it can fail ends as a non-zero exit code here, never as an
# exception that would end this hook with exit 1, which Claude Code treats as non-blocking. In-process, with its
# streams redirected, Windows PowerShell turns any stderr line from a tool the gate calls into a terminating error
# under the gate's ErrorActionPreference=Stop. The child's Write-Host lines arrive on its stdout and its stderr
# lines as error records; "$_" keeps each as the plain line it was.
$ErrorActionPreference = 'Continue'
$output = & powershell -NoProfile -ExecutionPolicy Bypass -File $GateScript -Configuration Debug 2>&1 |
    ForEach-Object { "$_" } | Out-String
if ($LASTEXITCODE -eq 0) { exit 0 }

$tail = ($output -split "`r?`n" | Select-Object -Last 60) -join "`n"
[Console]::Error.WriteLine("Local gate failed. Fix this before finishing:`n$tail")
exit 2
