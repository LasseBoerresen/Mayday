<#
.SYNOPSIS
    Claude Code PostToolUse hook: runs the complexity gate after Claude edits a C# file.

.DESCRIPTION
    Reads the hook input JSON from stdin and ignores edits to anything but .cs files. For a .cs edit it runs
    complexity-gate.ps1 and, when that fails, exits 2 with the gate's message on stderr, which Claude Code feeds
    back to Claude while the file is still in its context.

.PARAMETER GateScript
    The gate to run. Defaults to complexity-gate.ps1 beside this script.

.PARAMETER ProjectRoot
    Directory to gate. Defaults to $env:CLAUDE_PROJECT_DIR, else the current directory.
#>
param(
    [string] $GateScript = (Join-Path $PSScriptRoot 'complexity-gate.ps1'),
    [string] $ProjectRoot = $(if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path })
)

$ErrorActionPreference = 'Stop'
$json = [Console]::In.ReadToEnd()
$filePath = try { ($json | ConvertFrom-Json).tool_input.file_path } catch { $null }
if (-not $filePath -or $filePath -notmatch '\.cs$') { exit 0 }

# A child process, not `&`: PowerShell would wrap the gate's stderr in decorated error records.
$psi = New-Object Diagnostics.ProcessStartInfo
$psi.FileName = 'powershell'
$psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$GateScript`" -ProjectRoot `"$ProjectRoot`""
$psi.UseShellExecute = $false
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$gate = [Diagnostics.Process]::Start($psi)
$stdout = $gate.StandardOutput.ReadToEndAsync()
$stderr = $gate.StandardError.ReadToEndAsync()
$gate.WaitForExit()
if ($gate.ExitCode -eq 0) { exit 0 }

[Console]::Error.WriteLine(($stderr.Result + $stdout.Result).TrimEnd())
exit 2
