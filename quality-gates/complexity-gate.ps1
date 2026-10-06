<#
.SYNOPSIS
    Builds a .NET project or solution and fails when a method exceeds the cyclomatic complexity threshold.

.DESCRIPTION
    The threshold lives in CodeMetricsConfig.txt and the rule's severity in .editorconfig (see quality-gates/install),
    so a plain `dotnet build` enforces the same limit. This script is the fast early warning: it builds, collects the
    CA1502 errors and exits 2 with them, which Claude Code feeds back to Claude. Any other build failure also exits 2,
    with the tail of the build output. Exits 0 otherwise.

    Baseline (ratchet) mode, for code that already has violations: when complexity-baseline.json exists next to the
    project (or with -InitBaseline) this script is the arbiter and the repository's own build must not fail on the
    recorded violations, so set CA1502 to severity `warning` in .editorconfig (the script itself builds with
    WarningsNotAsErrors=CA1502, so it works either way). It tolerates the recorded violations, fails on a new
    violation or on a recorded method whose complexity grew, and reports improvements. The baseline maps
    `relative/path.cs::MethodName` to the complexities of that name's violations in that file (one per overload),
    highest first. Line numbers are not recorded, so unrelated edits do not disturb it; moving or renaming a method
    makes it look new.

.PARAMETER ProjectRoot
    Directory to build. Defaults to the current directory.

.PARAMETER Target
    Solution or project file to build, relative to ProjectRoot. Defaults to the only .sln, else the only .csproj.

.PARAMETER BaselinePath
    The baseline file. Defaults to complexity-baseline.json in ProjectRoot.

.PARAMETER InitBaseline
    Writes the baseline from the current violations. Refuses to overwrite an existing baseline.

.PARAMETER UpdateBaseline
    Lowers the baseline to the current state: records improvements and drops fixed methods. Refuses, leaving the file
    untouched, when there is a new violation or a regression, so it can never raise the limit.
#>
[CmdletBinding()]
param(
    [string] $ProjectRoot = (Get-Location).Path,
    [string] $Target,
    [string] $BaselinePath,
    [switch] $InitBaseline,
    [switch] $UpdateBaseline
)

$ErrorActionPreference = 'Stop'
Set-Location $ProjectRoot
if (-not $BaselinePath) { $BaselinePath = Join-Path $ProjectRoot 'complexity-baseline.json' }

function Stop-Gate([string] $Message) {
    [Console]::Error.WriteLine($Message)
    exit 2
}

function Get-BuildTarget {
    if ($Target) { return $Target }
    $found = @(Get-ChildItem -Path . -Filter *.sln)
    if ($found.Count -eq 0) { $found = @(Get-ChildItem -Path . -Filter *.csproj) }
    if ($found.Count -ne 1) { Stop-Gate "Cannot pick a build target in $ProjectRoot (found $($found.Count)); pass -Target." }
    $found[0].Name
}

# The CA1502 diagnostics of a build, as objects with the baseline Key and the Complexity.
function Get-Violations([string] $BuildOutput, [string] $Severity) {
    $root = (Get-Item -LiteralPath $ProjectRoot).FullName.TrimEnd('\') + '\'
    $pattern = "^(?<file>.+?)\((?<line>\d+),\d+\): $Severity CA1502: '(?<name>[^']+)' has a cyclomatic complexity of '(?<cx>\d+)'"
    $BuildOutput -split "`r?`n" | ForEach-Object {
        if ($_ -match $pattern) {
            $file = $Matches.file
            if ($file.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) { $file = $file.Substring($root.Length) }
            [pscustomobject]@{
                Key        = ($file -replace '\\', '/') + '::' + $Matches.name
                Line       = [int] $Matches.line
                Complexity = [int] $Matches.cx
            }
        }
    } | Sort-Object Key, Line, Complexity -Unique
}

# Key -> complexities, highest first.
function Group-Violations($Violations) {
    $groups = [ordered]@{}
    foreach ($group in ($Violations | Group-Object Key | Sort-Object Name)) {
        $groups[$group.Name] = @($group.Group | ForEach-Object Complexity | Sort-Object -Descending)
    }
    $groups
}

function Read-Baseline {
    $methods = (Get-Content -LiteralPath $BaselinePath -Raw | ConvertFrom-Json).methods
    $baseline = [ordered]@{}
    foreach ($property in $methods.PSObject.Properties) { $baseline[$property.Name] = @($property.Value | ForEach-Object { [int] $_ }) }
    $baseline
}

function Write-Baseline($Methods) {
    [ordered]@{ methods = $Methods } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $BaselinePath
}

$baselineMode = $InitBaseline -or $UpdateBaseline -or (Test-Path -LiteralPath $BaselinePath)
if ($InitBaseline -and (Test-Path -LiteralPath $BaselinePath)) { Stop-Gate "$BaselinePath already exists; use -UpdateBaseline to lower it." }
if ($UpdateBaseline -and -not (Test-Path -LiteralPath $BaselinePath)) { Stop-Gate "$BaselinePath does not exist; use -InitBaseline to create it." }

$buildArguments = @((Get-BuildTarget), '--nologo', '--verbosity', 'quiet', '--no-incremental')
# Projects that treat warnings as errors must not turn the tolerated baseline violations into build failures.
if ($baselineMode) { $buildArguments += '-p:WarningsNotAsErrors=CA1502' }
$output = dotnet build @buildArguments 2>&1 | Out-String
$buildExit = $LASTEXITCODE

if (-not $baselineMode) {
    if ($buildExit -eq 0) { exit 0 }
    $violations = $output -split "`r?`n" | Where-Object { $_ -match 'error CA1502' } | Sort-Object -Unique
    if ($violations) {
        Stop-Gate "Cyclomatic complexity gate failed. Refactor these methods (extract methods, replace branches with data or polymorphism):`n$($violations -join "`n")"
    }
    $tail = ($output -split "`r?`n" | Select-Object -Last 40) -join "`n"
    Stop-Gate "Build failed:`n$tail"
}

if ($buildExit -ne 0) {
    $tail = ($output -split "`r?`n" | Select-Object -Last 40) -join "`n"
    Stop-Gate "Build failed:`n$tail"
}

$current = Group-Violations (Get-Violations $output 'warning')

if ($InitBaseline) {
    Write-Baseline $current
    Write-Host "Baseline written: $($current.Count) method name(s) in $BaselinePath"
    exit 0
}

$baseline = Read-Baseline
$problems = @()
foreach ($key in $current.Keys) {
    $allowed = if ($baseline.Contains($key)) { $baseline[$key] } else { @() }
    for ($i = 0; $i -lt $current[$key].Count; $i++) {
        $value = $current[$key][$i]
        if ($i -ge $allowed.Count) { $problems += "NEW      $key has complexity $value (limit applies, not in baseline)" }
        elseif ($value -gt $allowed[$i]) { $problems += "GREW     $key complexity $value, baseline allows $($allowed[$i])" }
    }
}
if ($problems) {
    Stop-Gate "Cyclomatic complexity ratchet failed. Refactor, do not raise the baseline:`n$($problems -join "`n")"
}

$improved = $false
foreach ($key in $baseline.Keys) {
    $now = if ($current.Contains($key)) { $current[$key] } else { @() }
    if ((@($now) -join ',') -ne (@($baseline[$key]) -join ',')) { $improved = $true }
}

if ($UpdateBaseline) {
    Write-Baseline $current
    Write-Host "Baseline lowered: $($current.Count) method name(s) remain in $BaselinePath"
}
elseif ($improved) {
    Write-Host "Complexity improved below the baseline. Lock it in: run complexity-gate.ps1 -UpdateBaseline and commit the file."
}
exit 0
